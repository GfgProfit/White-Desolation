public sealed partial class CrateUIController
{
    private void TransferPlayerItemToCrate()
    {
        int slotIndex = _playerSelection.SelectedSlotIndex;

        CrateTransferService.TryMoveFromInventoryToCrate(_inventoryController, _activeCrate, slotIndex);
    }

    private void TransferCrateItemToPlayer()
    {
        int slotIndex = _crateSelection.SelectedSlotIndex;

        CrateTransferService.TryMoveFromCrateToInventory(_activeCrate, _inventoryController, slotIndex);
    }
}
