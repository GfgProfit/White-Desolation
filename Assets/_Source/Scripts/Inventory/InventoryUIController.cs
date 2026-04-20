using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class InventoryUIController : MonoBehaviour
{
    private const float ZeroTolerance = 0.0001f;

    private static readonly Color SelectedCategoryButtonColor = new Color32(0x30, 0x3B, 0x37, 0xFF); // #303B37
    private static readonly Color UnselectedCategoryButtonColor = new Color32(0x19, 0x1D, 0x1E, 0xFF); // #191D1E

    [Header("Root")]
    [SerializeField] private GameObject _inventoryRoot;

    [Header("Grid")]
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private InventoryItemCellView _cellPrefab;

    [Header("Category Filters")]
    [SerializeField] private CategoryFilterButton[] _categoryFilterButtons;

    [Header("Sort")]
    [SerializeField] private SortButtonConfig[] _sortButtons;
    [SerializeField] private InventorySortMode _defaultSortMode = InventorySortMode.Name;
    [SerializeField] private InventorySortDirection _defaultSortDirection = InventorySortDirection.Ascending;
    [SerializeField, Range(0f, 1f)] private float _selectedSortButtonAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float _unselectedSortButtonAlpha = 0.2f;

    [Header("Weight Display")]
    [SerializeField] private TMP_Text _carryWeightText;
    [SerializeField] private TMP_Text _currentWeightText;
    [SerializeField] private Slider _carryWeightSlider;

    [Header("Details")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemDescriptionText;
    [SerializeField] private TMP_Text _itemCountText;

    [Header("Stats")]
    [SerializeField] private GameObject _durabilityHolder;
    [SerializeField] private TMP_Text _durabilityText;
    [SerializeField] private Image _durabilityIcon;
    [SerializeField] private GameObject _weightHolder;
    [SerializeField] private TMP_Text _weightText;
    [SerializeField] private GameObject _caloriesHolder;
    [SerializeField] private TMP_Text _caloriesText;
    [SerializeField] private GameObject _hydrationHolder;
    [SerializeField] private TMP_Text _hydrationText;

    [Header("Buttons")]
    [SerializeField] private Button _useButton;
    [SerializeField] private TMP_Text _useButtonLabel;
    [SerializeField] private Button _dropOneButton;

    [Header("Use Progress Modal")]
    [SerializeField] private GameObject _useProgressModalRoot;
    [SerializeField] private Image _useProgressFillImage;
    [SerializeField] private TMP_Text _useProgressText;
    [SerializeField, Min(0.01f)] private float _useDurationSeconds = 5f;

    [Header("Scene References")]
    [SerializeField] private PlayerNeedsController _playerNeedsController;

    [Header("Optional")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [SerializeField] private GameObject[] _obectDisableWhileOpen;

    [Inject] private readonly InventoryController _inventoryController;
    [Inject] private readonly IPlayerInput _playerInput;

    private bool _isOpen;
    private bool _isUsingItem;
    private int _selectedIndex = -1;
    private InventoryCategoryFilter _activeFilter = InventoryCategoryFilter.All;
    private readonly List<SortButtonBinding> _sortButtonBindings = new();
    private readonly List<InventoryItemCellView> _spawnedCells = new();
    private readonly List<int> _visibleSlotIndices = new();
    private InventorySortMode _activeSortMode = InventorySortMode.None;
    private InventorySortDirection _activeSortDirection = InventorySortDirection.Ascending;
    private Coroutine _useRoutine;

    private void Awake()
    {
        if (_inventoryRoot != null)
        {
            _inventoryRoot.SetActive(false);
        }

        SetUseProgressVisible(false);
        SetUseProgress(0f, string.Empty);

        if (_useButton != null)
        {
            _useButton.onClick.AddListener(HandleUseClicked);
        }

        if (_dropOneButton != null)
        {
            _dropOneButton.onClick.AddListener(HandleDropOneClicked);
        }

        InitializeCategoryFilterButtons();
        RefreshCategoryFilterButtonVisuals();

        InitializeSortButtons();

        _activeSortMode = _defaultSortMode;
        _activeSortDirection = _defaultSortDirection;

        RefreshSortButtonVisuals();
    }

    private void Start()
    {
        if (_inventoryController == null)
        {
            return;
        }

        _inventoryController.OnInventoryChanged += RefreshView;
        RefreshView();
    }

    private void OnDestroy()
    {
        if (_inventoryController != null)
        {
            _inventoryController.OnInventoryChanged -= RefreshView;
        }

        if (_useButton != null)
        {
            _useButton.onClick.RemoveListener(HandleUseClicked);
        }

        if (_dropOneButton != null)
        {
            _dropOneButton.onClick.RemoveListener(HandleDropOneClicked);
        }

        CleanupSortButtons();

        if (_useRoutine != null)
        {
            StopCoroutine(_useRoutine);
        }
    }

    private void Update()
    {
        if (_playerInput == null)
        {
            return;
        }

        if (_isUsingItem)
        {
            return;
        }

        if (!_isOpen && _playerInput.IsInventoryPressed())
        {
            Open();
            return;
        }

        if (_isOpen && (_playerInput.IsInventoryPressed() || _playerInput.IsEscapePressed()))
        {
            Close();
        }
    }

    private void Open()
    {
        _isOpen = true;

        if (_inventoryRoot != null)
        {
            _inventoryRoot.SetActive(true);
        }

        SetBlockedBehaviours(false);
        SetBlockedObjects(false);
        SetCursorState(true);
        RefreshView();
    }

    private void Close()
    {
        if (_isUsingItem)
        {
            return;
        }

        _isOpen = false;

        if (_inventoryRoot != null)
        {
            _inventoryRoot.SetActive(false);
        }

        SetBlockedBehaviours(true);
        SetBlockedObjects(true);
        SetCursorState(false);
    }

    private void RefreshView()
    {
        if (_inventoryController == null)
        {
            return;
        }

        RefreshCarryWeight();
        RebuildVisibleSlotIndices();
        SortVisibleSlotIndices();
        ValidateSelectedIndexForCurrentFilter();
        RebuildGrid();
        RefreshDetails();
        RefreshCategoryFilterButtonVisuals();
        RefreshSortButtonVisuals();
    }

    private void InitializeSortButtons()
    {
        CleanupSortButtons();

        if (_sortButtons == null)
        {
            return;
        }

        for (int i = 0; i < _sortButtons.Length; i++)
        {
            SortButtonConfig config = _sortButtons[i];

            if (config.Button == null)
            {
                continue;
            }

            InventorySortMode mode = config.Mode;
            void action() => HandleSortButtonClicked(mode);

            config.Button.onClick.AddListener(action);
            _sortButtonBindings.Add(new SortButtonBinding(config.Button, action));
        }
    }

    private void CleanupSortButtons()
    {
        for (int i = 0; i < _sortButtonBindings.Count; i++)
        {
            SortButtonBinding binding = _sortButtonBindings[i];

            if (binding.Button != null && binding.Action != null)
            {
                binding.Button.onClick.RemoveListener(binding.Action);
            }
        }

        _sortButtonBindings.Clear();
    }

    private void HandleSortButtonClicked(InventorySortMode mode)
    {
        if (mode == InventorySortMode.None)
        {
            return;
        }

        if (_activeSortMode == mode)
        {
            _activeSortDirection = _activeSortDirection == InventorySortDirection.Ascending ? InventorySortDirection.Descending : InventorySortDirection.Ascending;
        }
        else
        {
            _activeSortMode = mode;
            _activeSortDirection = InventorySortDirection.Ascending;
        }

        RefreshView();
    }

    private void RefreshSortButtonVisuals()
    {
        if (_sortButtons == null)
        {
            return;
        }

        for (int i = 0; i < _sortButtons.Length; i++)
        {
            SortButtonConfig config = _sortButtons[i];
            bool isSelected = _activeSortMode != InventorySortMode.None && config.Mode == _activeSortMode;

            if (config.CanvasGroup != null)
            {
                config.CanvasGroup.alpha = isSelected ? _selectedSortButtonAlpha : _unselectedSortButtonAlpha;
            }
        }
    }

    private void SortVisibleSlotIndices()
    {
        if (_inventoryController == null)
        {
            return;
        }

        if (_activeSortMode == InventorySortMode.None)
        {
            return;
        }

        if (_visibleSlotIndices.Count <= 1)
        {
            return;
        }

        _visibleSlotIndices.Sort(CompareVisibleSlotIndices);
    }

    private int CompareVisibleSlotIndices(int leftSourceIndex, int rightSourceIndex)
    {
        if (_inventoryController == null)
        {
            return leftSourceIndex.CompareTo(rightSourceIndex);
        }

        InventorySlot leftSlot = _inventoryController.GetSlotAt(leftSourceIndex);
        InventorySlot rightSlot = _inventoryController.GetSlotAt(rightSourceIndex);

        if (leftSlot == null && rightSlot == null)
        {
            return leftSourceIndex.CompareTo(rightSourceIndex);
        }

        if (leftSlot == null)
        {
            return 1;
        }

        if (rightSlot == null)
        {
            return -1;
        }

        switch (_activeSortMode)
        {
            case InventorySortMode.Name:
                return CompareSlotsByName(leftSlot, leftSourceIndex, rightSlot, rightSourceIndex);

            case InventorySortMode.Durability:
                return CompareSlotsByDurability(leftSlot, leftSourceIndex, rightSlot, rightSourceIndex);

            case InventorySortMode.Weight:
                return CompareSlotsByWeight(leftSlot, leftSourceIndex, rightSlot, rightSourceIndex);
            default:
                return leftSourceIndex.CompareTo(rightSourceIndex);
        }
    }

    private int CompareSlotsByName(InventorySlot leftSlot, int leftSourceIndex, InventorySlot rightSlot, int rightSourceIndex)
    {
        string leftName = leftSlot.Item != null ? leftSlot.Item.DisplayName : string.Empty;
        string rightName = rightSlot.Item != null ? rightSlot.Item.DisplayName : string.Empty;

        int compare = string.Compare(leftName, rightName, StringComparison.CurrentCultureIgnoreCase);

        if (_activeSortDirection == InventorySortDirection.Descending)
        {
            compare = -compare;
        }

        if (compare != 0)
        {
            return compare;
        }

        return leftSourceIndex.CompareTo(rightSourceIndex);
    }

    private int CompareSlotsByDurability(InventorySlot leftSlot, int leftSourceIndex, InventorySlot rightSlot, int rightSourceIndex)
    {
        bool leftHasDurability = leftSlot.HasDurability;
        bool rightHasDurability = rightSlot.HasDurability;

        if (leftHasDurability != rightHasDurability)
        {
            return leftHasDurability ? -1 : 1;
        }

        if (leftHasDurability && rightHasDurability)
        {
            int compare = leftSlot.Durability01.CompareTo(rightSlot.Durability01);

            if (_activeSortDirection == InventorySortDirection.Descending)
            {
                compare = -compare;
            }

            if (compare != 0)
            {
                return compare;
            }
        }

        return CompareByNameThenSourceIndex(leftSlot, leftSourceIndex, rightSlot, rightSourceIndex);
    }

    private int CompareSlotsByWeight(InventorySlot leftSlot, int leftSourceIndex, InventorySlot rightSlot, int rightSourceIndex)
    {
        float leftWeight = InventoryWeightCalculator.GetSlotWeightKg(leftSlot);
        float rightWeight = InventoryWeightCalculator.GetSlotWeightKg(rightSlot);

        int compare = leftWeight.CompareTo(rightWeight);

        if (_activeSortDirection == InventorySortDirection.Descending)
        {
            compare = -compare;
        }

        if (compare != 0)
        {
            return compare;
        }

        return CompareByNameThenSourceIndex(leftSlot, leftSourceIndex, rightSlot, rightSourceIndex);
    }

    private int CompareByNameThenSourceIndex(InventorySlot leftSlot, int leftSourceIndex, InventorySlot rightSlot, int rightSourceIndex)
    {
        string leftName = leftSlot.Item != null ? leftSlot.Item.DisplayName : string.Empty;
        string rightName = rightSlot.Item != null ? rightSlot.Item.DisplayName : string.Empty;

        int compare = string.Compare(leftName, rightName, StringComparison.CurrentCultureIgnoreCase);

        if (compare != 0)
        {
            return compare;
        }

        return leftSourceIndex.CompareTo(rightSourceIndex);
    }

    private void RefreshCarryWeight()
    {
        if (_carryWeightText == null || _inventoryController == null)
        {
            return;
        }

        _carryWeightText.text = InventoryDisplayFormatter.FormatCarryWeight(_inventoryController.CurrentCarryWeightKg, _inventoryController.MaxCarryWeightKg);

        if (_currentWeightText != null)
        {
            _currentWeightText.text = InventoryDisplayFormatter.FormatCarryWeight(_inventoryController.CurrentCarryWeightKg, 0f);
        }

        if (_carryWeightSlider != null)
        {
            _carryWeightSlider.maxValue = _inventoryController.MaxCarryWeightKg;
            _carryWeightSlider.value = _inventoryController.CurrentCarryWeightKg;
        }
    }

    private void RebuildGrid()
    {
        for (int i = 0; i < _spawnedCells.Count; i++)
        {
            if (_spawnedCells[i] != null)
            {
                Destroy(_spawnedCells[i].gameObject);
            }
        }

        _spawnedCells.Clear();

        if (_inventoryController == null || _cellPrefab == null || _gridRoot == null)
        {
            return;
        }

        for (int i = 0; i < _visibleSlotIndices.Count; i++)
        {
            int sourceSlotIndex = _visibleSlotIndices[i];
            InventorySlot slot = _inventoryController.GetSlotAt(sourceSlotIndex);

            InventoryItemCellView cell = Instantiate(_cellPrefab, _gridRoot);
            cell.Bind(slot, sourceSlotIndex, sourceSlotIndex == _selectedIndex, HandleSlotSelected);
            _spawnedCells.Add(cell);
        }
    }

    private void HandleSlotSelected(int slotIndex)
    {
        if (_isUsingItem)
        {
            return;
        }

        _selectedIndex = slotIndex;

        for (int i = 0; i < _spawnedCells.Count; i++)
        {
            bool isSelected = i < _visibleSlotIndices.Count && _visibleSlotIndices[i] == _selectedIndex;
            _spawnedCells[i].SetSelected(isSelected);
        }

        RefreshDetails();
    }

    private void InitializeCategoryFilterButtons()
    {
        if (_categoryFilterButtons == null)
        {
            return;
        }

        for (int i = 0; i < _categoryFilterButtons.Length; i++)
        {
            CategoryFilterButton config = _categoryFilterButtons[i];

            if (config.Button == null)
            {
                continue;
            }

            InventoryCategoryFilter filter = config.Filter;
            config.Button.onClick.AddListener(() => HandleCategoryFilterClicked(filter));
        }
    }

    private void HandleCategoryFilterClicked(InventoryCategoryFilter filter)
    {
        if (_activeFilter == filter)
        {
            RefreshCategoryFilterButtonVisuals();
            return;
        }

        _activeFilter = filter;
        RefreshView();
    }

    private void RefreshCategoryFilterButtonVisuals()
    {
        if (_categoryFilterButtons == null)
        {
            return;
        }

        for (int i = 0; i < _categoryFilterButtons.Length; i++)
        {
            CategoryFilterButton config = _categoryFilterButtons[i];
            bool isSelected = config.Filter == _activeFilter;

            if (config.RootImage != null)
            {
                config.RootImage.color = isSelected ? SelectedCategoryButtonColor : UnselectedCategoryButtonColor;
            }

            if (config.IconCanvasGroup != null)
            {
                config.IconCanvasGroup.alpha = isSelected ? 1f : 0.2f;
            }
        }
    }

    private void RebuildVisibleSlotIndices()
    {
        _visibleSlotIndices.Clear();

        if (_inventoryController == null)
        {
            return;
        }

        for (int i = 0; i < _inventoryController.SlotCount; i++)
        {
            InventorySlot slot = _inventoryController.GetSlotAt(i);

            if (ShouldShowSlotForCurrentFilter(slot))
            {
                _visibleSlotIndices.Add(i);
            }
        }
    }

    private void ValidateSelectedIndexForCurrentFilter()
    {
        if (_visibleSlotIndices.Count == 0)
        {
            _selectedIndex = -1;
            return;
        }

        if (_selectedIndex >= 0 && _visibleSlotIndices.Contains(_selectedIndex))
        {
            return;
        }

        _selectedIndex = _visibleSlotIndices[0];
    }

    private bool ShouldShowSlotForCurrentFilter(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return false;
        }

        return IsCategoryAllowedByCurrentFilter(slot.Item.Category);
    }

    private bool IsCategoryAllowedByCurrentFilter(ItemCategory category)
    {
        switch (_activeFilter)
        {
            case InventoryCategoryFilter.All:
                return true;
            case InventoryCategoryFilter.MiscAndFuel:
                return category == ItemCategory.Misc || category == ItemCategory.Fuel;
            case InventoryCategoryFilter.Medical:
                return category == ItemCategory.Medical;
            case InventoryCategoryFilter.Clothing:
                return category == ItemCategory.Clothing;
            case InventoryCategoryFilter.FoodAndWater:
                return category == ItemCategory.Food || category == ItemCategory.Water;
            case InventoryCategoryFilter.ToolWeaponAndAmmo:
                return category == ItemCategory.Tool || category == ItemCategory.Weapon || category == ItemCategory.Ammo;
            case InventoryCategoryFilter.Resources:
                return category == ItemCategory.Resource;
            default:
                return true;
        }
    }

    private void RefreshDetails()
    {
        InventorySlot slot = _inventoryController != null ? _inventoryController.GetSlotAt(_selectedIndex) : null;

        bool hasSelection = slot != null && !slot.IsEmpty && slot.Item != null;
        bool canUseSelected = hasSelection && CanUseSlot(slot);
        bool canDrop = hasSelection && !_isUsingItem;

        if (_useButton != null)
        {
            _useButton.interactable = canUseSelected;
        }

        if (_dropOneButton != null)
        {
            _dropOneButton.interactable = canDrop;
        }

        if (_useButtonLabel != null)
        {
            _useButtonLabel.text = hasSelection ? InventoryDisplayFormatter.FormatPrimaryActionLabel(slot) : "Использовать";
        }

        if (!hasSelection)
        {
            if (_itemIcon != null)
            {
                _itemIcon.enabled = false;
                _itemIcon.sprite = null;
            }

            if (_itemNameText != null)
            {
                _itemNameText.text = "Не выбран предмет.";
            }

            if (_itemDescriptionText != null)
            {
                _itemDescriptionText.text = string.Empty;
            }

            if (_itemCountText != null)
            {
                _itemCountText.text = string.Empty;
            }

            ClearStats();
            return;
        }

        if (_itemIcon != null)
        {
            _itemIcon.enabled = slot.Item.Icon != null;
            _itemIcon.sprite = slot.Item.Icon;
        }

        if (_itemNameText != null)
        {
            _itemNameText.text = slot.Item.DisplayName;
        }

        if (_itemDescriptionText != null)
        {
            _itemDescriptionText.text = slot.Item.Description;
        }

        if (_itemCountText != null)
        {
            _itemCountText.text = InventoryDisplayFormatter.FormatPrimaryValue(slot);
        }

        RefreshStats(slot);
    }

    private void RefreshStats(InventorySlot slot)
    {
        SetStatRow(_durabilityHolder, _durabilityText, InventoryDisplayFormatter.TryGetDurabilityText(slot, out string durabilityText), durabilityText);

        Utils.SetDurabilityColor(slot, _durabilityText, _durabilityIcon);

        SetStatRow(_weightHolder, _weightText, InventoryDisplayFormatter.TryGetWeightText(slot, out string weightText), weightText);

        SetStatRow(_caloriesHolder, _caloriesText, InventoryDisplayFormatter.TryGetCaloriesText(slot, out string caloriesText), caloriesText);

        SetStatRow(_hydrationHolder, _hydrationText, InventoryDisplayFormatter.TryGetHydrationText(slot, out string hydrationText), hydrationText);
    }

    private void ClearStats()
    {
        SetStatRow(_durabilityHolder, _durabilityText, false, string.Empty);
        SetStatRow(_weightHolder, _weightText, false, string.Empty);
        SetStatRow(_caloriesHolder, _caloriesText, false, string.Empty);
        SetStatRow(_hydrationHolder, _hydrationText, false, string.Empty);
    }

    private void SetStatRow(GameObject holder, TMP_Text textComponent, bool visible, string value)
    {
        if (holder != null)
        {
            holder.SetActive(visible);
        }

        if (textComponent != null)
        {
            textComponent.text = visible ? value : string.Empty;
        }
    }

    private void HandleUseClicked()
    {
        if (_inventoryController == null || _isUsingItem)
        {
            return;
        }

        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);

        if (slot == null || slot.Item == null)
        {
            return;
        }

        if (!CanUseSlot(slot))
        {
            LogUseBlockedReason(slot);
            return;
        }

        if (!TryBuildUsePlan(_selectedIndex, slot, out UsePlan plan))
        {
            return;
        }

        _useRoutine = StartCoroutine(ExecuteUseRoutine(plan));
    }

    private IEnumerator ExecuteUseRoutine(UsePlan plan)
    {
        _isUsingItem = true;
        RefreshDetails();

        SetUseProgressVisible(true);
        SetUseProgress(0f, plan.VerbText);

        float elapsed = 0f;
        float appliedHydration = 0f;
        float appliedCalories = 0f;

        while (elapsed < plan.Duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / plan.Duration);
            SetUseProgress(progress, plan.VerbText);

            float targetHydration = plan.HydrationToApply * progress;
            float targetCalories = plan.CaloriesToApply * progress;

            float hydrationDelta = targetHydration - appliedHydration;
            float caloriesDelta = targetCalories - appliedCalories;

            if (Mathf.Abs(hydrationDelta) > ZeroTolerance)
            {
                float actualHydrationDelta = ApplyHydrationDelta(hydrationDelta);
                appliedHydration += actualHydrationDelta;
            }

            if (Mathf.Abs(caloriesDelta) > ZeroTolerance)
            {
                float actualCaloriesDelta = ApplyCaloriesDelta(caloriesDelta);
                appliedCalories += actualCaloriesDelta;
            }

            yield return null;
        }

        SetUseProgress(1f, plan.VerbText);

        if (plan.HasToolDurabilityConsume)
        {
            bool toolConsumed = _inventoryController.TryConsumeDurabilityFromFirstMatchingItem(plan.ToolItemToDamage, plan.ToolDurabilityCost);

            if (!toolConsumed)
            {
                SetUseProgressVisible(false);
                SetUseProgress(0f, string.Empty);

                _isUsingItem = false;
                _useRoutine = null;
                RefreshView();
                yield break;
            }
        }

        if (plan.ReplaceSlotItemAfterAction != null)
        {
            _inventoryController.TryReplaceSlotItem(plan.SlotIndex, plan.ReplaceSlotItemAfterAction);
        }

        if (plan.HasInventoryConsume)
        {
            _inventoryController.TryConsumeFromSlot(plan.SlotIndex, plan.HydrationStateToConsume, plan.CaloriesStateToConsume, plan.AmountToConsume, plan.ReplaceWhenDepleted);
        }

        SetUseProgressVisible(false);
        SetUseProgress(0f, string.Empty);
        RefreshView();

        if (plan.AutoUseReplacedItem)
        {
            InventorySlot nextSlot = _inventoryController.GetSlotAt(plan.SlotIndex);

            if (nextSlot != null && nextSlot.Item != null && TryBuildUsePlan(plan.SlotIndex, nextSlot, out UsePlan nextPlan))
            {
                _useRoutine = StartCoroutine(ExecuteUseRoutine(nextPlan));
                yield break;
            }
        }

        _isUsingItem = false;
        _useRoutine = null;
        RefreshView();
    }

    private float ApplyHydrationDelta(float hydrationDelta)
    {
        if (_playerNeedsController == null)
        {
            return 0f;
        }

        if (Mathf.Abs(hydrationDelta) <= ZeroTolerance)
        {
            return 0f;
        }

        if (hydrationDelta > 0f)
        {
            return _playerNeedsController.RestoreThirstUpTo(hydrationDelta);
        }

        float before = _playerNeedsController.Thirst;
        _playerNeedsController.AddThirst(hydrationDelta);
        return _playerNeedsController.Thirst - before;
    }

    private float ApplyCaloriesDelta(float caloriesDelta)
    {
        if (_playerNeedsController == null)
        {
            return 0f;
        }

        if (Mathf.Abs(caloriesDelta) <= ZeroTolerance)
        {
            return 0f;
        }

        if (caloriesDelta > 0f)
        {
            return _playerNeedsController.RestoreHungerUpTo(caloriesDelta);
        }

        float before = _playerNeedsController.Hunger;
        _playerNeedsController.AddHunger(caloriesDelta);
        return _playerNeedsController.Hunger - before;
    }

    private float RestoreHydrationDelta(float hydrationDelta)
    {
        if (_playerNeedsController == null)
        {
            return 0f;
        }

        return _playerNeedsController.RestoreThirstUpTo(hydrationDelta);
    }

    private float RestoreCaloriesDelta(float caloriesDelta)
    {
        if (_playerNeedsController == null)
        {
            return 0f;
        }

        return _playerNeedsController.RestoreHungerUpTo(caloriesDelta);
    }

    private bool TryBuildUsePlan(int slotIndex, InventorySlot slot, out UsePlan plan)
    {
        plan = default;

        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (slot.Item.RequiresOpening)
        {
            return TryBuildOpenPlan(slotIndex, slot, out plan);
        }

        plan.SlotIndex = slotIndex;
        plan.ActionType = slot.Item.PrimaryAction;
        plan.VerbText = ResolveUseVerb(slot);
        plan.Duration = _useDurationSeconds;

        switch (slot.Item.PrimaryAction)
        {
            case ItemPrimaryActionType.Use:
                return TryBuildConsumableUsePlan(slot, ref plan);
            case ItemPrimaryActionType.Action:
                return true;
            default:
                return false;
        }
    }

    private bool TryBuildOpenPlan(int slotIndex, InventorySlot slot, out UsePlan plan)
    {
        plan = default;

        if (_inventoryController == null || slot == null || slot.Item == null)
        {
            return false;
        }

        ItemData item = slot.Item;

        if (!item.RequiresOpening || item.AfterOpen == null)
        {
            return false;
        }

        if (!_inventoryController.ContainsUsableItem(item.NeedsToOpen))
        {
            return false;
        }

        plan.SlotIndex = slotIndex;
        plan.ActionType = ItemPrimaryActionType.Action;
        plan.VerbText = "открывает";
        plan.Duration = _useDurationSeconds;

        plan.ReplaceSlotItemAfterAction = item.AfterOpen;
        plan.AutoUseReplacedItem = true;

        plan.ToolItemToDamage = item.NeedsToOpen;
        plan.ToolDurabilityCost = item.NeedsToOpenDurabilityCost;
        return true;
    }

    private bool TryBuildConsumableUsePlan(InventorySlot slot, ref UsePlan plan)
    {
        if (_playerNeedsController == null)
        {
            return false;
        }

        if (IsVolumeDrink(slot))
        {
            float hydrationToApply = Mathf.Min(slot.CurrentAmount, _playerNeedsController.MissingThirst);

            if (hydrationToApply <= ZeroTolerance)
            {
                return false;
            }

            plan.HydrationToApply = hydrationToApply;
            plan.AmountToConsume = hydrationToApply;
            return true;
        }

        float useRatio = CalculateConsumableUseRatio(slot);

        if (useRatio <= ZeroTolerance)
        {
            return false;
        }

        if (Mathf.Abs(slot.CurrentHydration) > ZeroTolerance)
        {
            float hydrationAmount = slot.CurrentHydration * useRatio;
            plan.HydrationToApply = hydrationAmount;
            plan.HydrationStateToConsume = hydrationAmount;
        }

        if (Mathf.Abs(slot.CurrentCalories) > ZeroTolerance)
        {
            float caloriesAmount = slot.CurrentCalories * useRatio;
            plan.CaloriesToApply = caloriesAmount;
            plan.CaloriesStateToConsume = caloriesAmount;
        }

        if (slot.HasAmount && slot.CurrentAmount > ZeroTolerance)
        {
            plan.AmountToConsume = slot.CurrentAmount * useRatio;
        }

        plan.ReplaceWhenDepleted = slot.Item.AfterUse;
        return plan.HasPlayerEffect || plan.HasInventoryConsume;
    }

    private float CalculateConsumableUseRatio(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || _playerNeedsController == null)
        {
            return 0f;
        }

        float ratio = 1f;
        bool hasPositiveEffect = false;
        bool hasAnyEffect = false;

        if (Mathf.Abs(slot.CurrentHydration) > ZeroTolerance)
        {
            hasAnyEffect = true;

            if (slot.CurrentHydration > ZeroTolerance)
            {
                hasPositiveEffect = true;
                ratio = Mathf.Min(ratio, _playerNeedsController.MissingThirst / slot.CurrentHydration);
            }
        }

        if (Mathf.Abs(slot.CurrentCalories) > ZeroTolerance)
        {
            hasAnyEffect = true;

            if (slot.CurrentCalories > 0f)
            {
                hasPositiveEffect = true;
                ratio = Mathf.Min(ratio, _playerNeedsController.MissingHunger / slot.CurrentCalories);
            }
        }

        if (!hasAnyEffect)
        {
            return 0f;
        }

        if (!hasPositiveEffect)
        {
            return 1f;
        }

        return Mathf.Clamp01(ratio);
    }

    private string ResolveUseVerb(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return "использует";
        }

        if (slot.Item.Category == ItemCategory.Water)
        {
            return "пьет";
        }

        if (slot.Item.Category == ItemCategory.Food)
        {
            return "ест";
        }

        if (slot.Item.Category == ItemCategory.Resource)
        {
            return "собирает";
        }

        if (slot.Item.Category == ItemCategory.Tool)
        {
            return "ремонтирует";
        }

        return "открывает";
    }

    private static bool IsVolumeDrink(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (slot.Item.PrimaryAction != ItemPrimaryActionType.Use)
        {
            return false;
        }

        if (slot.Item.Category != ItemCategory.Water)
        {
            return false;
        }

        if (!slot.HasAmount)
        {
            return false;
        }

        if (slot.Item.AmountUnit != ItemAmountUnit.Liter)
        {
            return false;
        }

        if (slot.CurrentAmount <= ZeroTolerance)
        {
            return false;
        }

        if (slot.Item.RestoreCalories > 0)
        {
            return false;
        }

        if (slot.CurrentCalories > ZeroTolerance)
        {
            return false;
        }

        return true;
    }

    private void SetUseProgressVisible(bool visible)
    {
        if (_useProgressModalRoot != null)
        {
            _useProgressModalRoot.SetActive(visible);
        }
    }

    private void SetUseProgress(float progress01, string text)
    {
        if (_useProgressFillImage != null)
        {
            _useProgressFillImage.fillAmount = Mathf.Clamp01(progress01);
        }

        if (_useProgressText != null)
        {
            _useProgressText.text = text;
        }
    }

    private bool CanUseSlot(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (_isUsingItem)
        {
            return false;
        }

        if (slot.IsBroken)
        {
            return slot.Item.PrimaryAction == ItemPrimaryActionType.Action;
        }

        return slot.Item.PrimaryAction switch
        {
            ItemPrimaryActionType.Use => CanUseConsumableSlot(slot),
            ItemPrimaryActionType.Action => true,
            _ => false
        };
    }

    private bool CanUseConsumableSlot(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || _playerNeedsController == null)
        {
            return false;
        }

        if (slot.Item.RequiresOpening)
        {
            return CanUseClosedConsumableSlot(slot);
        }

        if (IsVolumeDrink(slot))
        {
            return slot.CurrentAmount > ZeroTolerance && _playerNeedsController.MissingThirst > ZeroTolerance;
        }

        bool hasHydrationEffect = Mathf.Abs(slot.CurrentHydration) > ZeroTolerance;
        bool hasCaloriesEffect = Mathf.Abs(slot.CurrentCalories) > ZeroTolerance;

        return hasHydrationEffect || hasCaloriesEffect;
    }

    private bool DoesAffectHydration(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (IsVolumeDrink(slot))
        {
            return true;
        }

        return Mathf.Abs(slot.CurrentHydration) > ZeroTolerance;
    }

    private bool DoesAffectCalories(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return false;
        }

        return Mathf.Abs(slot.CurrentCalories) > ZeroTolerance;
    }

    private void LogUseBlockedReason(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return;
        }

        if (slot.IsBroken)
        {
            return;
        }

        if (_playerNeedsController == null)
        {
            return;
        }

        if (slot.Item.RequiresOpening && _inventoryController != null && !_inventoryController.ContainsUsableItem(slot.Item.NeedsToOpen))
        {
            return;
        }
    }

    private void HandleDropOneClicked()
    {
        if (_isUsingItem)
        {
            return;
        }

        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);

        if (slot == null || slot.Item == null)
        {
            return;
        }

        _inventoryController.TryRemoveFromSlot(_selectedIndex, 1);
    }

    private void SetBlockedBehaviours(bool enabled)
    {
        if (_disableWhileOpen == null)
        {
            return;
        }

        for (int i = 0; i < _disableWhileOpen.Length; i++)
        {
            if (_disableWhileOpen[i] != null)
            {
                _disableWhileOpen[i].enabled = enabled;
            }
        }
    }

    private void SetBlockedObjects(bool enabled)
    {
        if (_obectDisableWhileOpen == null)
        {
            return;
        }

        for (int i = 0; i < _obectDisableWhileOpen.Length; i++)
        {
            if (_obectDisableWhileOpen[i] != null)
            {
                _obectDisableWhileOpen[i].SetActive(enabled);
            }
        }
    }

    private bool CanUseClosedConsumableSlot(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || _inventoryController == null)
        {
            return false;
        }

        ItemData item = slot.Item;

        if (!item.RequiresOpening)
        {
            return false;
        }

        if (item.AfterOpen == null || item.AfterOpen == item || item.AfterOpen.RequiresOpening)
        {
            return false;
        }

        if (!_inventoryController.ContainsUsableItem(item.NeedsToOpen))
        {
            return false;
        }

        InventorySlot previewOpenedSlot = new();
        previewOpenedSlot.Initialize(item.AfterOpen, 1);

        return CanUseConsumableSlot(previewOpenedSlot);
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}