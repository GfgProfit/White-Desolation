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
            slot.CurrentHydration -= hydrationToConsume;

            if (Mathf.Abs(slot.CurrentHydration) <= zeroTolerance)
            {
                slot.CurrentHydration = 0f;
            }
        }

        if (!Mathf.Approximately(caloriesToConsume, 0f))
        {
            slot.CurrentCalories -= caloriesToConsume;

            if (Mathf.Abs(slot.CurrentCalories) <= zeroTolerance)
            {
                slot.CurrentCalories = 0f;
            }
        }

        if (!Mathf.Approximately(amountToConsume, 0f))
        {
            slot.CurrentAmount = Mathf.Max(0f, slot.CurrentAmount - amountToConsume);
        }
    }
}