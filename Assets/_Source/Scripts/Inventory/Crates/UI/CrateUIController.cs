using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CrateUIController : MonoBehaviour
{
    private const float WeightWarningOffsetKg = 5f;

    [Header("Auto References")]
    [SerializeField] private InventoryUIController _inventoryUIController;
    [SerializeField] private InteractController _interactController;
    [SerializeField] private GameObject _inventoryRightPanel;
    [SerializeField] private GameObject _rightCratePanel;

    [Header("Search Progress")]
    [SerializeField] private GameObject _searchProgressRoot;
    [SerializeField] private Image _searchProgressFillImage;
    [SerializeField] private TMP_Text _searchProgressText;
    [SerializeField] private string _searchProgressLabel = "Поиск";

    [Header("Take Item Window")]
    [SerializeField] private GameObject _takeItemRoot;
    [SerializeField] private Image _takeItemIcon;
    [SerializeField] private Image _takeItemDurabilityIcon;
    [SerializeField] private TMP_Text _takeItemNameText;
    [SerializeField] private TMP_Text _takeItemDescriptionText;
    [SerializeField] private TMP_Text _takeItemDurabilityText;
    [SerializeField] private TMP_Text _takeItemWeightText;

    [Header("Crate Window")]
    [SerializeField] private GameObject _crateRoot;
    [SerializeField] private Transform _playerGridRoot;
    [SerializeField] private Transform _crateGridRoot;
    [SerializeField] private InventoryItemCellView _cellPrefab;
    [SerializeField] private Button _crateActionButton;
    [SerializeField] private TMP_Text _crateWeightText;
    [SerializeField] private Slider _crateWeightSlider;

    [Header("Optional")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [SerializeField] private GameObject[] _objectDisableWhileOpen;

    [Inject] private InventoryController _inventoryController = null;
    [Inject] private IPlayerInput _playerInput = null;

    private readonly List<InventoryItemCellView> _playerCells = new();
    private readonly List<InventoryItemCellView> _crateCells = new();
    private readonly List<InventoryViewEntry> _playerEntries = new();
    private readonly List<InventoryViewEntry> _crateEntries = new();
    private readonly List<InventorySlot> _searchedSlots = new();
    private readonly InventorySelectionState _playerSelection = new();
    private readonly InventorySelectionState _crateSelection = new();

    private InventoryUseProgressModalPresenter _searchProgress;
    private InteractionInspectPresenter _takeItemPresenter;
    private CrateContainer _activeCrate;
    private Coroutine _searchRoutine;
    private int _searchedSlotCursor;
    private CrateSelectionSource _selectionSource;
    private bool _isCrateOpen;
    private bool _isBrowsingSearchResults;
    private bool _hasCrateWindowSnapshot;
    private bool _crateRootWasActive;
    private bool _rightCratePanelWasActive;
    private bool _inventoryRightPanelWasActive;
    private bool _inventoryUIControllerWasEnabled;

    private void Awake()
    {
        EnsureRuntimeReferences();
        AutoWireSceneReferences();

        _searchProgress = new InventoryUseProgressModalPresenter(_searchProgressRoot, _searchProgressFillImage, _searchProgressText);
        _takeItemPresenter = new InteractionInspectPresenter(_takeItemRoot, _takeItemIcon, _takeItemDurabilityIcon, _takeItemNameText, _takeItemDescriptionText, _takeItemDurabilityText, _takeItemWeightText);

        _searchProgress.InitializeHidden();
        _takeItemPresenter.Hide();

        if (_rightCratePanel != null)
        {
            _rightCratePanel.SetActive(false);
        }

        if (_crateActionButton != null)
        {
            _crateActionButton.onClick.AddListener(HandleCrateActionClicked);
        }
    }

    private void OnDestroy()
    {
        if (_crateActionButton != null)
        {
            _crateActionButton.onClick.RemoveListener(HandleCrateActionClicked);
        }

        StopSearchRoutine();
        CloseCrate();
        CloseBrowsing();

        InventoryGridRenderer.Clear(_playerCells);
        InventoryGridRenderer.Clear(_crateCells);
    }

    private void Update()
    {
        EnsureRuntimeReferences();

        if (_isBrowsingSearchResults)
        {
            HandleBrowseInput();
            return;
        }

        if (_isCrateOpen && _playerInput != null && (_playerInput.IsEscapePressed() || _playerInput.IsInventoryPressed()))
        {
            CloseCrate();
        }
    }

    public void BeginSearch(CrateContainer crate)
    {
        if (crate == null)
        {
            return;
        }

        EnsureRuntimeReferences();
        AutoWireSceneReferences();

        CloseCrate();
        CloseBrowsing();
        StopSearchRoutine();

        _activeCrate = crate;
        _searchRoutine = StartCoroutine(SearchRoutine(crate));
    }

    public void OpenCrate(CrateContainer crate)
    {
        if (crate == null)
        {
            return;
        }

        EnsureRuntimeReferences();
        AutoWireSceneReferences();

        if (_inventoryController == null)
        {
            Debug.LogWarning("[Crate] Cannot open crate UI without InventoryController.", this);
            return;
        }

        StopSearchRoutine();
        CloseBrowsing();

        if (_activeCrate != null)
        {
            _activeCrate.OnChanged -= RefreshCrateWindow;
        }

        _activeCrate = crate;
        _activeCrate.OnChanged += RefreshCrateWindow;

        if (_inventoryUIController != null)
        {
            _inventoryUIController.ExternalGridRefreshRequested -= RefreshCrateWindow;
            _inventoryUIController.ExternalGridRefreshRequested += RefreshCrateWindow;
        }

        _isCrateOpen = true;

        PrepareCrateWindowObjects();

        PlayerControlLockService.Lock(this, _disableWhileOpen, _objectDisableWhileOpen);
        CursorLockService.ShowCursor(this);

        _selectionSource = CrateSelectionSource.None;
        _playerSelection.Clear();
        _crateSelection.Clear();

        RefreshCrateWindow();
    }

    public void CloseCrate()
    {
        if (!_isCrateOpen)
        {
            return;
        }

        _isCrateOpen = false;

        if (_activeCrate != null)
        {
            _activeCrate.OnChanged -= RefreshCrateWindow;
        }

        if (_inventoryUIController != null)
        {
            _inventoryUIController.ExternalGridRefreshRequested -= RefreshCrateWindow;
        }

        InventoryGridRenderer.Clear(_playerCells);
        InventoryGridRenderer.Clear(_crateCells);

        _playerEntries.Clear();
        _crateEntries.Clear();
        _playerSelection.Clear();
        _crateSelection.Clear();
        _selectionSource = CrateSelectionSource.None;

        RestoreCrateWindowObjects();

        PlayerControlLockService.Unlock(this, _disableWhileOpen, _objectDisableWhileOpen);
        CursorLockService.ReleaseCursor(this);
    }

    private IEnumerator SearchRoutine(CrateContainer crate)
    {
        PlayerControlLockService.Lock(this, _disableWhileOpen, _objectDisableWhileOpen);
        _searchProgress.Show(_searchProgressLabel);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, crate.SearchDurationSeconds);

        while (elapsed < duration)
        {
            if (_playerInput == null || !_playerInput.IsInteractHold())
            {
                CompleteSearchCancellation();
                yield break;
            }

            elapsed += Time.deltaTime;
            _searchProgress.UpdateProgress(Mathf.Clamp01(elapsed / duration), _searchProgressLabel);

            yield return null;
        }

        _searchProgress.Complete(_searchProgressLabel);
        _searchProgress.HideAndReset();
        _searchRoutine = null;

        crate.MarkSearched();
        PlayerControlLockService.Unlock(this, _disableWhileOpen, _objectDisableWhileOpen);

        BeginBrowseSearchResults(crate);
    }

    private void BeginBrowseSearchResults(CrateContainer crate)
    {
        _searchedSlots.Clear();
        _searchedSlotCursor = 0;

        for (int i = 0; i < crate.Items.Count; i++)
        {
            InventorySlot slot = crate.Items[i];

            if (slot != null && !slot.IsEmpty && slot.Item != null)
            {
                _searchedSlots.Add(slot);
            }
        }

        if (_searchedSlots.Count == 0)
        {
            CloseBrowsing();
            return;
        }

        _activeCrate = crate;
        _isBrowsingSearchResults = true;
        PlayerControlLockService.Lock(this, _disableWhileOpen, _objectDisableWhileOpen);
        ShowCurrentBrowseSlot();
    }

    private void HandleBrowseInput()
    {
        if (_playerInput == null)
        {
            return;
        }

        if (_playerInput.IsInteractPressed())
        {
            TryTakeCurrentBrowseSlot();
            AdvanceBrowseSlot();
            return;
        }

        if (_playerInput.IsInteractDenied())
        {
            AdvanceBrowseSlot();
        }
    }

    private void ShowCurrentBrowseSlot()
    {
        InventorySlot slot = GetCurrentBrowseSlot();

        if (slot == null || slot.Item == null)
        {
            AdvanceBrowseSlot();
            return;
        }

        InteractionInspectInfo info = WorldItemInteractionInfoBuilder.BuildInspectInfo(slot.Item, slot.CurrentDurability, InventoryWeightCalculator.GetSlotWeightKg(slot));
        _takeItemPresenter.Show(info);
    }

    private void TryTakeCurrentBrowseSlot()
    {
        InventorySlot slot = GetCurrentBrowseSlot();

        if (slot == null || slot.Item == null || _activeCrate == null || _inventoryController == null)
        {
            return;
        }

        bool added = _inventoryController.TryAddItem(
            slot.Item,
            slot.Count,
            slot.HasAmount ? slot.CurrentAmount : null,
            slot.HasDurability ? slot.CurrentDurability : null,
            slot.HasConsumableState ? slot.CurrentHydration : null,
            slot.HasConsumableState ? slot.CurrentCalories : null);

        if (added)
        {
            _activeCrate.TryRemoveFromSlot(slot, slot.Count);
        }
    }

    private void AdvanceBrowseSlot()
    {
        _searchedSlotCursor++;

        if (_searchedSlotCursor >= _searchedSlots.Count)
        {
            CloseBrowsing();
            return;
        }

        ShowCurrentBrowseSlot();
    }

    private InventorySlot GetCurrentBrowseSlot()
    {
        if (_searchedSlotCursor < 0 || _searchedSlotCursor >= _searchedSlots.Count)
        {
            return null;
        }

        return _searchedSlots[_searchedSlotCursor];
    }

    private void CloseBrowsing()
    {
        if (!_isBrowsingSearchResults)
        {
            _takeItemPresenter?.Hide();
            return;
        }

        _isBrowsingSearchResults = false;
        _searchedSlots.Clear();
        _searchedSlotCursor = 0;
        _takeItemPresenter?.Hide();
        PlayerControlLockService.Unlock(this, _disableWhileOpen, _objectDisableWhileOpen);
    }

    private void StopSearchRoutine()
    {
        if (_searchRoutine == null)
        {
            return;
        }

        StopCoroutine(_searchRoutine);
        CompleteSearchCancellation();
    }

    private void CompleteSearchCancellation()
    {
        _searchRoutine = null;
        _searchProgress?.HideAndReset();
        PlayerControlLockService.Unlock(this, _disableWhileOpen, _objectDisableWhileOpen);
    }

    private void RefreshCrateWindow()
    {
        if (!_isCrateOpen || _activeCrate == null || _inventoryController == null)
        {
            return;
        }

        int preferredPlayerVisibleIndex = _playerSelection.GetVisibleIndex(_playerEntries);
        int preferredCrateVisibleIndex = _crateSelection.GetVisibleIndex(_crateEntries);
        InventorySlot preferredPlayerSlot = _playerSelection.GetSelectedSlot(_playerEntries);
        InventorySlot preferredCrateSlot = _crateSelection.GetSelectedSlot(_crateEntries);

        BuildPlayerEntries();
        BuildCrateEntries();

        _playerSelection.ValidateForVisibleEntries(_playerEntries, preferredPlayerVisibleIndex, preferredPlayerSlot);
        _crateSelection.ValidateForVisibleEntries(_crateEntries, preferredCrateVisibleIndex, preferredCrateSlot);

        if (_selectionSource == CrateSelectionSource.None)
        {
            if (_playerSelection.HasSelection)
            {
                _selectionSource = CrateSelectionSource.PlayerInventory;
                _crateSelection.Clear();
            }
            else if (_crateSelection.HasSelection)
            {
                _selectionSource = CrateSelectionSource.CrateInventory;
                _playerSelection.Clear();
            }
        }

        InventoryGridRenderer.Rebuild(_cellPrefab, _playerGridRoot, _playerCells, _playerEntries, _selectionSource == CrateSelectionSource.PlayerInventory ? _playerSelection : null, HandlePlayerSlotSelected);
        InventoryGridRenderer.Rebuild(_cellPrefab, _crateGridRoot, _crateCells, _crateEntries, _selectionSource == CrateSelectionSource.CrateInventory ? _crateSelection : null, HandleCrateSlotSelected);

        RefreshWeight();
        RefreshActionButton();
    }

    private void BuildPlayerEntries()
    {
        if (_inventoryUIController != null)
        {
            InventoryViewQuery.BuildVisibleEntries(
                _inventoryController,
                _inventoryUIController.ActiveFilter,
                _inventoryUIController.ActiveSortMode,
                _inventoryUIController.ActiveSortDirection,
                _playerEntries);

            return;
        }

        _playerEntries.Clear();

        for (int i = 0; i < _inventoryController.SlotCount; i++)
        {
            InventorySlot slot = _inventoryController.GetSlotAt(i);

            if (slot != null && !slot.IsEmpty && slot.Item != null)
            {
                _playerEntries.Add(new InventoryViewEntry(i, slot));
            }
        }
    }

    private void BuildCrateEntries()
    {
        if (_inventoryUIController != null)
        {
            InventoryViewQuery.BuildVisibleEntries(
                _activeCrate.Items,
                _inventoryUIController.ActiveFilter,
                _inventoryUIController.ActiveSortMode,
                _inventoryUIController.ActiveSortDirection,
                _crateEntries);

            return;
        }

        _crateEntries.Clear();

        for (int i = 0; i < _activeCrate.Items.Count; i++)
        {
            InventorySlot slot = _activeCrate.Items[i];

            if (slot != null && !slot.IsEmpty && slot.Item != null)
            {
                _crateEntries.Add(new InventoryViewEntry(i, slot));
            }
        }
    }

    private void HandlePlayerSlotSelected(int slotIndex)
    {
        _selectionSource = CrateSelectionSource.PlayerInventory;
        _playerSelection.SelectSlot(slotIndex);
        _crateSelection.Clear();

        InventoryGridRenderer.RefreshSelection(_playerCells, _playerEntries, _playerSelection);
        InventoryGridRenderer.RefreshSelection(_crateCells, _crateEntries, _crateSelection);

        RefreshActionButton();
    }

    private void HandleCrateSlotSelected(int slotIndex)
    {
        _selectionSource = CrateSelectionSource.CrateInventory;
        _crateSelection.SelectSlot(slotIndex);
        _playerSelection.Clear();

        InventoryGridRenderer.RefreshSelection(_playerCells, _playerEntries, _playerSelection);
        InventoryGridRenderer.RefreshSelection(_crateCells, _crateEntries, _crateSelection);

        RefreshActionButton();
    }

    private void HandleCrateActionClicked()
    {
        if (_activeCrate == null || _inventoryController == null)
        {
            return;
        }

        if (_selectionSource == CrateSelectionSource.PlayerInventory)
        {
            TransferPlayerItemToCrate();
        }
        else if (_selectionSource == CrateSelectionSource.CrateInventory)
        {
            TransferCrateItemToPlayer();
        }

        RefreshCrateWindow();
    }

    private void TransferPlayerItemToCrate()
    {
        int slotIndex = _playerSelection.SelectedSlotIndex;
        InventorySlot slot = _inventoryController.GetSlotAt(slotIndex);

        if (slot == null || slot.Item == null)
        {
            return;
        }

        if (!_activeCrate.TryAddFromSlot(slot, 1))
        {
            return;
        }

        if (!_inventoryController.TryRemoveFromSlot(slotIndex, 1))
        {
            _activeCrate.TryRemoveFromSlot(_activeCrate.Items.Count - 1, 1);
        }
    }

    private void TransferCrateItemToPlayer()
    {
        int slotIndex = _crateSelection.SelectedSlotIndex;
        InventorySlot slot = GetCrateSlot(slotIndex);

        if (slot == null || slot.Item == null)
        {
            return;
        }

        bool added = _inventoryController.TryAddItem(
            slot.Item,
            1,
            slot.HasAmount ? slot.CurrentAmount : null,
            slot.HasDurability ? slot.CurrentDurability : null,
            slot.HasConsumableState ? slot.CurrentHydration : null,
            slot.HasConsumableState ? slot.CurrentCalories : null);

        if (added)
        {
            _activeCrate.TryRemoveFromSlot(slotIndex, 1);
        }
    }

    private InventorySlot GetCrateSlot(int slotIndex)
    {
        if (_activeCrate == null || slotIndex < 0 || slotIndex >= _activeCrate.Items.Count)
        {
            return null;
        }

        return _activeCrate.Items[slotIndex];
    }

    private void RefreshWeight()
    {
        if (_activeCrate == null)
        {
            return;
        }

        float currentWeight = _activeCrate.CurrentWeightKg;
        float maxWeight = _activeCrate.MaxWeightKg;

        if (_crateWeightText != null)
        {
            _crateWeightText.text = FormatCrateWeight(currentWeight, maxWeight);
        }

        if (_crateWeightSlider != null)
        {
            _crateWeightSlider.maxValue = maxWeight;
            _crateWeightSlider.value = currentWeight;
        }
    }

    private void RefreshActionButton()
    {
        if (_crateActionButton == null)
        {
            return;
        }

        bool hasSelection = _selectionSource == CrateSelectionSource.PlayerInventory && _playerSelection.HasSelection
            || _selectionSource == CrateSelectionSource.CrateInventory && _crateSelection.HasSelection;

        _crateActionButton.interactable = hasSelection;

        Transform actionTransform = _crateActionButton.transform;
        Vector3 eulerAngles = actionTransform.localEulerAngles;
        eulerAngles.z = _selectionSource == CrateSelectionSource.CrateInventory ? 180f : 0f;
        actionTransform.localEulerAngles = eulerAngles;
    }

    private void EnsureRuntimeReferences()
    {
        if (_inventoryController == null)
        {
            _inventoryController = FindFirstObjectByType<InventoryController>(FindObjectsInactive.Include);
        }

        _playerInput ??= new LegacyPlayerInput();
    }

    private void AutoWireSceneReferences()
    {
        if (_inventoryUIController == null)
        {
            _inventoryUIController = FindFirstObjectByType<InventoryUIController>(FindObjectsInactive.Include);
        }

        if (_interactController == null)
        {
            _interactController = FindFirstObjectByType<InteractController>(FindObjectsInactive.Include);
        }

        if (_inventoryUIController != null)
        {
            _crateRoot ??= _inventoryUIController.InventoryRoot;
            _playerGridRoot ??= _inventoryUIController.GridRoot;
            _cellPrefab ??= _inventoryUIController.CellPrefab;
            _searchProgressRoot ??= _inventoryUIController.UseProgressModalRoot;
            _searchProgressFillImage ??= _inventoryUIController.UseProgressFillImage;
            _searchProgressText ??= _inventoryUIController.UseProgressText;

            if (IsNullOrEmpty(_disableWhileOpen))
            {
                _disableWhileOpen = _inventoryUIController.DisableWhileOpen;
            }

            if (IsNullOrEmpty(_objectDisableWhileOpen))
            {
                _objectDisableWhileOpen = _inventoryUIController.ObjectDisableWhileOpen;
            }
        }

        if (_interactController != null)
        {
            _takeItemRoot ??= _interactController.InspectRoot;
            _takeItemIcon ??= _interactController.InspectIcon;
            _takeItemDurabilityIcon ??= _interactController.InspectDurabilityIcon;
            _takeItemNameText ??= _interactController.InspectNameText;
            _takeItemDescriptionText ??= _interactController.InspectDescriptionText;
            _takeItemDurabilityText ??= _interactController.InspectDurabilityText;
            _takeItemWeightText ??= _interactController.InspectWeightText;
        }

        _crateRoot ??= FindSceneGameObject("InventoryRoot");
        _rightCratePanel ??= FindSceneGameObject("Right Crate Panel");
        _inventoryRightPanel ??= FindSceneGameObject("Right Panel");

        if (_crateRoot == null && _rightCratePanel != null)
        {
            _crateRoot = _rightCratePanel;
        }

        if (_rightCratePanel != null)
        {
            _crateGridRoot ??= FindDeepChildByPath(_rightCratePanel.transform, "Scroll View", "Viewport", "Content") ?? FindDeepChild(_rightCratePanel.transform, "Content");
            _crateActionButton ??= FindComponentInChildrenByName<Button>(_rightCratePanel, "Crate Action Button");
            _crateWeightText ??= FindComponentInChildrenByName<TMP_Text>(_rightCratePanel, "Weight Text");
            _crateWeightSlider ??= FindComponentInChildrenByName<Slider>(_rightCratePanel, "Weight Slider");
        }
    }

    private void PrepareCrateWindowObjects()
    {
        if (!_hasCrateWindowSnapshot)
        {
            _crateRootWasActive = _crateRoot != null && _crateRoot.activeSelf;
            _rightCratePanelWasActive = _rightCratePanel != null && _rightCratePanel.activeSelf;
            _inventoryRightPanelWasActive = _inventoryRightPanel != null && _inventoryRightPanel.activeSelf;
            _inventoryUIControllerWasEnabled = _inventoryUIController != null && _inventoryUIController.enabled;
            _hasCrateWindowSnapshot = true;
        }

        if (_inventoryUIController != null)
        {
            _inventoryUIController.ReleaseGridForExternalUse();
            _inventoryUIController.enabled = false;
        }

        if (_crateRoot != null)
        {
            _crateRoot.SetActive(true);
        }

        if (_inventoryRightPanel != null)
        {
            _inventoryRightPanel.SetActive(false);
        }

        if (_rightCratePanel != null)
        {
            _rightCratePanel.SetActive(true);
        }
    }

    private void RestoreCrateWindowObjects()
    {
        if (!_hasCrateWindowSnapshot)
        {
            return;
        }

        if (_rightCratePanel != null)
        {
            _rightCratePanel.SetActive(_rightCratePanelWasActive);
        }

        if (_inventoryRightPanel != null)
        {
            _inventoryRightPanel.SetActive(_inventoryRightPanelWasActive);
        }

        if (_crateRoot != null)
        {
            _crateRoot.SetActive(_crateRootWasActive);
        }

        if (_inventoryUIController != null)
        {
            _inventoryUIController.enabled = _inventoryUIControllerWasEnabled;

            if (_inventoryUIControllerWasEnabled)
            {
                _inventoryUIController.RefreshAfterExternalUse();
            }
        }

        _hasCrateWindowSnapshot = false;
    }

    private static string FormatCrateWeight(float currentWeightKg, float maxWeightKg)
    {
        if (maxWeightKg <= 0f)
        {
            return $"{currentWeightKg:0.##} КГ";
        }

        string maxText = $"{maxWeightKg:0.##}";

        if (currentWeightKg >= Mathf.Max(0f, maxWeightKg - WeightWarningOffsetKg))
        {
            maxText = $"<color=#9E2F3C>{maxText}</color>";
        }

        return $"{currentWeightKg:0.##} / {maxText} КГ";
    }

    private static bool IsNullOrEmpty<T>(T[] array)
    {
        return array == null || array.Length == 0;
    }

    private static GameObject FindSceneGameObject(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform target = transforms[i];

            if (target == null || target.gameObject == null || !target.gameObject.scene.IsValid())
            {
                continue;
            }

            if (target.name == objectName)
            {
                return target.gameObject;
            }
        }

        return null;
    }

    private static Transform FindDeepChildByPath(Transform root, params string[] path)
    {
        if (root == null || path == null || path.Length == 0)
        {
            return null;
        }

        Transform current = root;

        for (int i = 0; i < path.Length; i++)
        {
            current = FindDirectChild(current, path[i]);

            if (current == null)
            {
                return null;
            }
        }

        return current;
    }

    private static Transform FindDirectChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);

            if (child != null && child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        if (root.name == childName)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeepChild(root.GetChild(i), childName);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static T FindComponentInChildrenByName<T>(GameObject root, string objectName) where T : Component
    {
        if (root == null || string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        T[] components = root.GetComponentsInChildren<T>(true);

        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];

            if (component != null && component.gameObject.name == objectName)
            {
                return component;
            }
        }

        return null;
    }
}
