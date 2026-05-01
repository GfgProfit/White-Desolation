using UnityEngine;

public partial class InventoryUIController
{
    private void Awake()
    {
        ResolveOptionalReferences();
        InitializeWindowState();
        InitializeUseServices();
        InitializeMainButtons();
        InitializeCategoryFilters();
        InitializeSorting();
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

    private void OnDisable()
    {
        _windowState?.ReleaseOwner();
        StopUseRoutineAndResetProgress();
    }

    private void OnDestroy()
    {
        CleanupInventoryEvents();
        CleanupMainButtons();
        CleanupCategoryFilterButtons();
        CleanupSortButtons();

        StopUseRoutineAndResetProgress();

        _windowState?.ReleaseOwner();

        InventoryGridRenderer.Clear(_spawnedCells);
    }

    private void Update()
    {
        if (_playerInput == null)
        {
            return;
        }

        if (_useRoutineState.IsUsingItem)
        {
            return;
        }

        if (_windowState != null && !_windowState.IsOpen && _playerInput.IsInventoryPressed())
        {
            Open();
            return;
        }

        if (_windowState != null && _windowState.IsOpen && (_playerInput.IsInventoryPressed() || _playerInput.IsEscapePressed()))
        {
            Close();
        }
    }

    private void InitializeWindowState()
    {
        _windowState = new InventoryWindowStateController(this, _inventoryRoot, _disableWhileOpen, _objectDisableWhileOpen);

        _windowState.InitializeClosed();
    }

    private void ResolveOptionalReferences()
    {
        if (_itemDropper == null)
        {
            _itemDropper = GetComponent<InventoryItemDropper>();
        }

        if (_itemDropper == null)
        {
            _itemDropper = gameObject.AddComponent<InventoryItemDropper>();
        }
    }

    private void InitializeUseServices()
    {
        _playerNeeds = _playerNeedsSource as IPlayerNeeds;

        if (_playerNeedsSource != null && _playerNeeds == null)
        {
            Debug.LogWarning($"{nameof(InventoryUIController)} requires a player needs source that implements {nameof(IPlayerNeeds)}.", this);
        }

        _useProgressModal = new InventoryUseProgressModalPresenter(_useProgressModalRoot, _useProgressFillImage, _useProgressText);

        _useProgressModal.InitializeHidden();

        _useProgressApplier = new InventoryUseProgressApplier(_playerNeeds, ZeroTolerance);

        _useCompletionService = new InventoryUseCompletionService(_inventoryController);

        _itemDropper?.SetInventoryController(_inventoryController);
    }

    private void StopUseRoutineAndResetProgress()
    {
        _useRoutineState.StopAndReset(this);
        _useProgressModal?.HideAndReset();
        _useProgressApplier?.Reset();
    }

    private void CleanupInventoryEvents()
    {
        if (_inventoryController != null)
        {
            _inventoryController.OnInventoryChanged -= RefreshView;
        }
    }
}
