using UnityEngine;

public static class InventoryStackMergePolicy
{
    public static bool CanMergeIntoStack(InventorySlot slot, ItemData incomingItem, float? incomingDurabilityOverride, float? incomingAmountOverride)
    {
        if (slot == null || slot.IsEmpty || slot.Item == null || incomingItem == null)
        {
            return false;
        }

        if (!ItemDataComparer.AreSame(slot.Item, incomingItem))
        {
            return false;
        }

        if (slot.UsesPerInstanceConsumableState || InventoryConsumableInstancePolicy.RequiresDedicatedInstance(incomingItem))
        {
            return false;
        }

        if (slot.Item.UsesCustomAmount || incomingItem.UsesCustomAmount)
        {
            float incomingAmount = Mathf.Clamp(incomingAmountOverride ?? incomingItem.MaxAmount, 0f, incomingItem.MaxAmount);

            return Mathf.Approximately(slot.CurrentAmount, incomingAmount);
        }

        if (slot.Item.UsesDurability && !slot.Item.IsUnbreakable)
        {
            float incomingDurability = Mathf.Clamp(incomingDurabilityOverride ?? incomingItem.MaxDurability, 0f, incomingItem.MaxDurability);

            return Mathf.Approximately(slot.CurrentDurability, incomingDurability);
        }

        return true;
    }
}