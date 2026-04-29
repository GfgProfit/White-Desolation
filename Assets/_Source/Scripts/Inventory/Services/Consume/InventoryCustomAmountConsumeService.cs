using System.Collections.Generic;
using UnityEngine;

public static class InventoryCustomAmountConsumeService
{
    public static bool TryConsumeAcrossSlots(List<InventorySlot> slots, ItemData itemData, float amount, float zeroTolerance)
    {
        if (slots == null || itemData == null || amount <= 0f)
        {
            return false;
        }

        if (InventoryItemQuery.GetTotalAmount(slots, itemData) + zeroTolerance < amount)
        {
            return false;
        }

        float remainingAmount = amount;

        for (int i = slots.Count - 1; i >= 0 && remainingAmount > zeroTolerance; i--)
        {
            InventorySlot slot = slots[i];

            if (!CanConsumeFromSlot(slot, itemData))
            {
                continue;
            }

            float amountToConsume = Mathf.Min(slot.CurrentAmount, remainingAmount);

            if (amountToConsume <= zeroTolerance)
            {
                continue;
            }

            InventorySlotConsumeApplier.ApplyConsume(slot, 0f, 0f, amountToConsume, zeroTolerance);
            InventoryConsumedSlotMutationService.RemoveOrReplaceIfDepleted(slots, i, slot, null);

            remainingAmount -= amountToConsume;
        }

        return remainingAmount <= zeroTolerance;
    }

    private static bool CanConsumeFromSlot(InventorySlot slot, ItemData itemData)
    {
        if (slot == null || slot.IsEmpty || !slot.HasAmount)
        {
            return false;
        }

        return ItemDataComparer.AreSame(slot.Item, itemData);
    }
}
