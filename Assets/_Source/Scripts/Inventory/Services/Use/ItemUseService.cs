public static class ItemUseService
{
    public static bool CanUseSlot(ItemUseContext context, InventorySlot slot)
    {
        return ItemUseAvailabilityPolicy.CanUseSlot(context, slot);
    }

    public static bool TryBuildPlan(ItemUseContext context, int slotIndex, InventorySlot slot, out ItemUsePlan plan)
    {
        return ItemUsePlanBuilder.TryBuildPlan(context, slotIndex, slot, out plan);
    }

    public static bool IsVolumeDrink(InventorySlot slot)
    {
        return ItemVolumeDrinkPolicy.IsVolumeDrink(slot);
    }
}
