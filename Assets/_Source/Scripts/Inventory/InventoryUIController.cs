using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    private const string DebugPrefix = "[InventoryUI]";

    [Header("Root")]
    [SerializeField] private GameObject _inventoryRoot;

    [Header("Grid")]
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private InventoryItemCellView _cellPrefab;

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
    [SerializeField] private Button _dropStackButton;

    [Header("Optional")]
    [SerializeField] private Behaviour[] _disableWhileOpen;

    [Inject] private InventoryController _inventoryController;
    [Inject] private IPlayerInput _playerInput;

    private readonly List<InventoryItemCellView> _spawnedCells = new();

    private bool _isOpen;
    private int _selectedIndex = -1;

    private void Awake()
    {
        if (_inventoryRoot != null)
            _inventoryRoot.SetActive(false);

        if (_useButton != null)
            _useButton.onClick.AddListener(HandleUseClicked);

        if (_dropOneButton != null)
            _dropOneButton.onClick.AddListener(HandleDropOneClicked);

        if (_dropStackButton != null)
            _dropStackButton.onClick.AddListener(HandleDropStackClicked);
    }

    private void Start()
    {
        if (_inventoryController == null)
        {
            Debug.LogError($"{DebugPrefix} InventoryController is null.");
            return;
        }

        _inventoryController.OnInventoryChanged += RefreshView;
        RefreshView();
    }

    private void OnDestroy()
    {
        if (_inventoryController != null)
            _inventoryController.OnInventoryChanged -= RefreshView;

        if (_useButton != null)
            _useButton.onClick.RemoveListener(HandleUseClicked);

        if (_dropOneButton != null)
            _dropOneButton.onClick.RemoveListener(HandleDropOneClicked);

        if (_dropStackButton != null)
            _dropStackButton.onClick.RemoveListener(HandleDropStackClicked);
    }

    private void Update()
    {
        if (_playerInput == null)
            return;

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
            _inventoryRoot.SetActive(true);

        SetBlockedBehaviours(false);
        SetCursorState(true);
        RefreshView();
    }

    private void Close()
    {
        _isOpen = false;

        if (_inventoryRoot != null)
            _inventoryRoot.SetActive(false);

        SetBlockedBehaviours(true);
        SetCursorState(false);
    }

    private void RefreshView()
    {
        if (_inventoryController == null)
            return;

        RefreshCarryWeight();

        if (_inventoryController.SlotCount == 0)
        {
            _selectedIndex = -1;
        }
        else
        {
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _inventoryController.SlotCount - 1);

            if (_selectedIndex < 0)
                _selectedIndex = 0;
        }

        RebuildGrid();
        RefreshDetails();
    }

    private void RefreshCarryWeight()
    {
        if (_carryWeightText == null || _inventoryController == null)
            return;

        _carryWeightText.text = InventoryDisplayFormatter.FormatCarryWeight(
            _inventoryController.CurrentCarryWeightKg,
            _inventoryController.MaxCarryWeightKg);

        _currentWeightText.text = InventoryDisplayFormatter.FormatCarryWeight(
            _inventoryController.CurrentCarryWeightKg, 0);

        _carryWeightSlider.maxValue = _inventoryController.MaxCarryWeightKg;
        _carryWeightSlider.value = _inventoryController.CurrentCarryWeightKg;
    }

    private void RebuildGrid()
    {
        for (int i = 0; i < _spawnedCells.Count; i++)
        {
            if (_spawnedCells[i] != null)
                Destroy(_spawnedCells[i].gameObject);
        }

        _spawnedCells.Clear();

        if (_inventoryController == null || _cellPrefab == null || _gridRoot == null)
            return;

        for (int i = 0; i < _inventoryController.SlotCount; i++)
        {
            InventorySlot slot = _inventoryController.GetSlotAt(i);
            InventoryItemCellView cell = Instantiate(_cellPrefab, _gridRoot);
            cell.Bind(slot, i, i == _selectedIndex, HandleSlotSelected);
            _spawnedCells.Add(cell);
        }
    }

    private void HandleSlotSelected(int slotIndex)
    {
        _selectedIndex = slotIndex;

        for (int i = 0; i < _spawnedCells.Count; i++)
        {
            _spawnedCells[i].SetSelected(i == _selectedIndex);
        }

        RefreshDetails();
    }

    private void RefreshDetails()
    {
        InventorySlot slot = _inventoryController != null
            ? _inventoryController.GetSlotAt(_selectedIndex)
            : null;

        bool hasSelection = slot != null && !slot.IsEmpty && slot.Item != null;
        bool hasPrimaryAction = hasSelection && slot.Item.PrimaryAction != ItemPrimaryActionType.None;

        if (_useButton != null)
            _useButton.interactable = hasPrimaryAction;

        if (_dropOneButton != null)
            _dropOneButton.interactable = hasSelection;

        if (_dropStackButton != null)
            _dropStackButton.interactable = hasSelection && (slot.Count > 1 || slot.HasAmount);

        if (_useButtonLabel != null)
            _useButtonLabel.text = hasSelection
                ? InventoryDisplayFormatter.FormatPrimaryActionLabel(slot)
                : "Использовать";

        if (!hasSelection)
        {
            if (_itemIcon != null)
            {
                _itemIcon.enabled = false;
                _itemIcon.sprite = null;
            }

            if (_itemNameText != null)
                _itemNameText.text = "Не выбран предмет.";

            if (_itemDescriptionText != null)
                _itemDescriptionText.text = string.Empty;

            if (_itemCountText != null)
                _itemCountText.text = string.Empty;

            ClearStats();
            return;
        }

        if (_itemIcon != null)
        {
            _itemIcon.enabled = slot.Item.Icon != null;
            _itemIcon.sprite = slot.Item.Icon;
        }

        if (_itemNameText != null)
            _itemNameText.text = slot.Item.DisplayName;

        if (_itemDescriptionText != null)
            _itemDescriptionText.text = slot.Item.Description;

        if (_itemCountText != null)
            _itemCountText.text = InventoryDisplayFormatter.FormatPrimaryValue(slot);

        RefreshStats(slot);
    }

    private void RefreshStats(InventorySlot slot)
    {
        SetStatRow(
            _durabilityHolder,
            _durabilityText,
            InventoryDisplayFormatter.TryGetDurabilityText(slot, out string durabilityText),
            durabilityText);

        SetStatRow(
            _weightHolder,
            _weightText,
            InventoryDisplayFormatter.TryGetWeightText(slot, out string weightText),
            weightText);

        SetStatRow(
            _caloriesHolder,
            _caloriesText,
            InventoryDisplayFormatter.TryGetCaloriesText(slot, out string caloriesText),
            caloriesText);

        SetStatRow(
            _hydrationHolder,
            _hydrationText,
            InventoryDisplayFormatter.TryGetHydrationText(slot, out string hydrationText),
            hydrationText);
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
            holder.SetActive(visible);

        if (textComponent != null)
            textComponent.text = visible ? value : string.Empty;
    }

    private void HandleUseClicked()
    {
        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);
        if (slot == null || slot.Item == null)
            return;

        switch (slot.Item.PrimaryAction)
        {
            case ItemPrimaryActionType.Use:
                Debug.Log($"{DebugPrefix} Use requested for {slot.Item.DisplayName}.");
                break;

            case ItemPrimaryActionType.Action:
                Debug.Log($"{DebugPrefix} Action requested for {slot.Item.DisplayName}.");
                break;

            default:
                Debug.Log($"{DebugPrefix} No primary action for {slot.Item.DisplayName}.");
                break;
        }
    }

    private void HandleDropOneClicked()
    {
        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);
        if (slot == null || slot.Item == null)
            return;

        _inventoryController.TryRemoveFromSlot(_selectedIndex, 1);
    }

    private void HandleDropStackClicked()
    {
        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);
        if (slot == null || slot.Item == null)
            return;

        _inventoryController.TryRemoveFromSlot(_selectedIndex, slot.Count);
    }

    private void SetBlockedBehaviours(bool enabled)
    {
        if (_disableWhileOpen == null)
            return;

        for (int i = 0; i < _disableWhileOpen.Length; i++)
        {
            if (_disableWhileOpen[i] != null)
                _disableWhileOpen[i].enabled = enabled;
        }
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}