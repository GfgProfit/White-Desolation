public static class InventoryAddCapacityPolicy
{
    public static bool CanAddItem(ItemData itemData, int count, float? currentAmountOverride = null)
    {
        if (itemData == null)
        {
            return false;
        }

        if (count <= 0)
        {
            return false;
        }

        if (itemData.UsesCustomAmount && itemData.MaxAmount <= 0f)
        {
            return false;
        }

        if (currentAmountOverride.HasValue && currentAmountOverride.Value <= 0f)
        {
            return false;
        }

        return true;
    }

    public static bool CanAddItem(float currentCarryWeightKg, float maxCarryWeightKg, ItemData itemData, int count, float? currentAmountOverride = null)
    {
        return CanAddItem(itemData, count, currentAmountOverride);
    }
}
