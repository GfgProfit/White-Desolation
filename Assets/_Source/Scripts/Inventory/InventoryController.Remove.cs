public partial class InventoryController
{
    public bool TryRemoveItem(ItemData itemData, int count)
    {
        bool removed = InventoryItemRemovalService.TryRemoveItem(_items, itemData, count);

        return FinishInventoryMutation(removed);
    }

    public bool TryRemoveFromSlot(int slotIndex, int count)
    {
        bool removed = InventorySlotRemovalService.TryRemoveFromSlot(_items, slotIndex, count);

        return FinishInventoryMutation(removed);
    }
}