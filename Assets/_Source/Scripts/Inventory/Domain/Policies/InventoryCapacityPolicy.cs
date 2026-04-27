public static class InventoryCapacityPolicy
{
    private const float ZeroTolerance = 0.0001f;

    public static bool CanAcceptWeight(float currentWeightKg, float maxCarryWeightKg, float incomingWeightKg)
    {
        if (incomingWeightKg <= ZeroTolerance)
        {
            return true;
        }

        if (maxCarryWeightKg <= ZeroTolerance)
        {
            return true;
        }

        return currentWeightKg + incomingWeightKg <= maxCarryWeightKg + ZeroTolerance;
    }
}