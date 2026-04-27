public partial class InventoryController
{
    public bool TryAddItem(ItemData itemData, int count, float? currentAmountOverride = null, float? currentDurabilityOverride = null)
    {
        if (itemData == null)
        {
            return false;
        }

        if (count <= 0)
        {
            return false;
        }

        if (itemData.UsesCustomAmount)
        {
            bool addedCustomAmount = InventoryCustomAmountAddService.TryAddCustomAmountItem(_items, itemData, count, currentAmountOverride, currentDurabilityOverride, ZeroTolerance);

            return FinishInventoryMutation(addedCustomAmount);
        }

        if (InventoryConsumableInstancePolicy.RequiresDedicatedInstance(itemData))
        {
            bool addedDedicatedConsumables = InventorySeparateSlotAddService.TryAddSeparateSlotItems(_items, itemData, count, currentDurabilityOverride, currentAmountOverride);

            return FinishInventoryMutation(addedDedicatedConsumables);
        }

        if (itemData.IsStackable)
        {
            bool addedStackableItems = InventoryStackableAddService.TryAddStackableItems(_items, itemData, count, currentDurabilityOverride, currentAmountOverride);

            return FinishInventoryMutation(addedStackableItems);
        }

        bool addedNonStackableItems = InventorySeparateSlotAddService.TryAddSeparateSlotItems(_items, itemData, count, currentDurabilityOverride, currentAmountOverride);

        return FinishInventoryMutation(addedNonStackableItems);
    }
}