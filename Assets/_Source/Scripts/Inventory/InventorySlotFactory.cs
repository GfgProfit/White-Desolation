public static class InventorySlotFactory
{
    public static InventorySlot Create(ItemData itemData, int count, float? currentDurabilityOverride = null, float? currentAmountOverride = null, float? currentHydrationOverride = null, float? currentCaloriesOverride = null)
    {
        InventorySlot slot = new();
        slot.Initialize(itemData, count, currentDurabilityOverride, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride);

        return slot;
    }
}