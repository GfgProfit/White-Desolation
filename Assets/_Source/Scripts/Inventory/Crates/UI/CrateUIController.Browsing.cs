public sealed partial class CrateUIController
{
    private void BeginBrowseSearchResults(CrateContainer crate)
    {
        CrateSearchResultQuery.BuildAvailableSlots(crate, _searchedSlots);
        _searchedSlotCursor = 0;

        if (_searchedSlots.Count == 0)
        {
            CloseBrowsing();
            return;
        }

        _activeCrate = crate;
        _isBrowsingSearchResults = true;
        LockPlayerControls();
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

        int slotIndex = _activeCrate.IndexOf(slot);

        if (slotIndex < 0)
        {
            return;
        }

        CrateTransferService.TryMoveFromCrateToInventory(_activeCrate, _inventoryController, slotIndex, slot.Count);
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
        UnlockPlayerControls();
    }
}
