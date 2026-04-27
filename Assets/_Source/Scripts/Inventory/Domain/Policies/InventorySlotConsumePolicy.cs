using UnityEngine;

public static class InventorySlotConsumePolicy
{
    private const float ZeroTolerance = 0.0001f;

    public static bool ShouldRemoveSlotAfterConsume(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return true;
        }

        if (slot.HasAmount)
        {
            return slot.CurrentAmount <= ZeroTolerance;
        }

        if (slot.UsesPerInstanceConsumableState)
        {
            return Mathf.Abs(slot.CurrentHydration) <= ZeroTolerance && Mathf.Abs(slot.CurrentCalories) <= ZeroTolerance;
        }

        return slot.IsEmpty;
    }
}