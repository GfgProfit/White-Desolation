using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    private const string DebugPrefix = "<color=green>[InventoryUI]</color>";

    [Header("Root")]
    [SerializeField] private GameObject _inventoryRoot;

    [Header("Grid")]
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private InventoryItemCellView _cellPrefab;

    [Header("Details")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemDescriptionText;
    [SerializeField] private TMP_Text _itemCountText;

    [Header("Buttons")]
    [SerializeField] private Button _useButton;
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

        _useButton.onClick.AddListener(HandleUseClicked);
        _dropOneButton.onClick.AddListener(HandleDropOneClicked);
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

        _useButton.onClick.RemoveListener(HandleUseClicked);
        _dropOneButton.onClick.RemoveListener(HandleDropOneClicked);
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
        _inventoryRoot.SetActive(true);

        SetBlockedBehaviours(false);
        SetCursorState(true);

        RefreshView();
    }

    private void Close()
    {
        _isOpen = false;
        _inventoryRoot.SetActive(false);

        SetBlockedBehaviours(true);
        SetCursorState(false);
    }

    private void RefreshView()
    {
        if (_inventoryController == null)
            return;

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

    private void RebuildGrid()
    {
        for (int i = 0; i < _spawnedCells.Count; i++)
        {
            if (_spawnedCells[i] != null)
                Destroy(_spawnedCells[i].gameObject);
        }

        _spawnedCells.Clear();

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
        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);

        bool hasSelection = slot != null && !slot.IsEmpty && slot.Item != null;

        _useButton.interactable = hasSelection;
        _dropOneButton.interactable = hasSelection;
        _dropStackButton.interactable = hasSelection;

        if (!hasSelection)
        {
            _itemIcon.enabled = false;
            _itemIcon.sprite = null;
            _itemNameText.text = "Не выбран предмет.";
            _itemDescriptionText.text = string.Empty;
            _itemCountText.text = string.Empty;
            return;
        }

        _itemIcon.enabled = slot.Item.Icon != null;
        _itemIcon.sprite = slot.Item.Icon;

        _itemNameText.text = slot.Item.DisplayName;
        _itemDescriptionText.text = slot.Item.Description;
        _itemCountText.text = $"x{slot.Count}";
    }

    private void HandleUseClicked()
    {
        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);
        if (slot == null || slot.Item == null)
            return;

        Debug.Log($"{DebugPrefix} Use requested for {slot.Item.DisplayName}.");
        // TODO: здесь позже подключается реальная логика использования предмета
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