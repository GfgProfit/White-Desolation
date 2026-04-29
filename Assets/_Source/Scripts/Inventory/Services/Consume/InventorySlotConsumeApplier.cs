using UnityEngine;

public static class InventorySlotConsumeApplier
{
    public static void ApplyConsume(InventorySlot slot, float hydrationToConsume, float caloriesToConsume, float amountToConsume, float zeroTolerance)
    {
        if (slot == null)
        {
            return;
        }

        if (!Mathf.Approximately(hydrationToConsume, 0f))
        {
            slot.ConsumeHydration(hydrationToConsume, zeroTolerance);
        }

        if (!Mathf.Approximately(caloriesToConsume, 0f))
        {
            slot.ConsumeCalories(caloriesToConsume, zeroTolerance);
        }

        if (!Mathf.Approximately(amountToConsume, 0f))
        {
            slot.ConsumeAmount(amountToConsume);
        }
    }
}
