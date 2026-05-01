public sealed partial class CrateUIController
{
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
            UnityEngine.Debug.LogWarning("[Crate] Cannot open crate UI without InventoryController.", this);
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

        LockPlayerControls();
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

        UnlockPlayerControls();
        CursorLockService.ReleaseCursor(this);
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

        _crateWindowPresenter?.RefreshWeight(_activeCrate);
        _crateWindowPresenter?.RefreshActionButton(_selectionSource, HasActiveTransferSelection());
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

        _crateWindowPresenter?.RefreshActionButton(_selectionSource, HasActiveTransferSelection());
    }

    private void HandleCrateSlotSelected(int slotIndex)
    {
        _selectionSource = CrateSelectionSource.CrateInventory;
        _crateSelection.SelectSlot(slotIndex);
        _playerSelection.Clear();

        InventoryGridRenderer.RefreshSelection(_playerCells, _playerEntries, _playerSelection);
        InventoryGridRenderer.RefreshSelection(_crateCells, _crateEntries, _crateSelection);

        _crateWindowPresenter?.RefreshActionButton(_selectionSource, HasActiveTransferSelection());
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

    private bool HasActiveTransferSelection()
    {
        return _selectionSource == CrateSelectionSource.PlayerInventory && _playerSelection.HasSelection
            || _selectionSource == CrateSelectionSource.CrateInventory && _crateSelection.HasSelection;
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
}
