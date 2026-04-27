public static class InventoryAddCapacityPolicy
{
    public static bool CanAddItem(float currentCarryWeightKg, float maxCarryWeightKg, ItemData itemData, int count, float? currentAmountOverride = null)
    {
        if (itemData == null)
        {
            return false;
        }

        if (count <= 0)
        {
            return false;
        }

        float incomingWeightKg = InventoryWeightCalculator.CalculateIncomingWeightKg(itemData, count, currentAmountOverride);

        return InventoryCapacityPolicy.CanAcceptWeight(currentCarryWeightKg, maxCarryWeightKg, incomingWeightKg);
    }
}