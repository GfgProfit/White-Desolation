public partial class InventoryController
{
    public bool TryReplaceSlotItem(int slotIndex, ItemData newItemData)
    {
        bool replaced = InventorySlotReplacementService.TryReplaceSlotItem(_items, slotIndex, newItemData);

        return FinishInventoryMutation(replaced);
    }
}