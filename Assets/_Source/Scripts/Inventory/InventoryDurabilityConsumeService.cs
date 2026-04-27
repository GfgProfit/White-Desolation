using System.Collections.Generic;
using UnityEngine;

public static class InventoryDurabilityConsumeService
{
    public static InventoryDurabilityConsumeResult TryConsumeFromFirstMatchingItem(IReadOnlyList<InventorySlot> slots, ItemData itemData, float durabilityCost)
    {
        if (slots == null || itemData == null)
        {
            return InventoryDurabilityConsumeResult.Failed;
        }

        if (durabilityCost <= 0f)
        {
            return InventoryItemQuery.Contains(slots, itemData) ? InventoryDurabilityConsumeResult.SucceededWithoutMutation : InventoryDurabilityConsumeResult.Failed;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                continue;
            }

            if (!ItemDataComparer.AreSame(slot.Item, itemData))
            {
                continue;
            }

            if (!slot.HasDurability)
            {
                return InventoryDurabilityConsumeResult.SucceededWithoutMutation;
            }

            if (slot.IsBroken)
            {
                continue;
            }

            slot.CurrentDurability = Mathf.Max(0f, slot.CurrentDurability - durabilityCost);

            return InventoryDurabilityConsumeResult.Mutated;
        }

        return InventoryDurabilityConsumeResult.Failed;
    }
}