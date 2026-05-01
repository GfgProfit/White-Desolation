using System.Collections.Generic;
using UnityEngine;

public static class InventoryDurabilityConsumeService
{
    public static InventoryDurabilityConsumeResult TryConsumeFromSlot(IReadOnlyList<InventorySlot> slots, int slotIndex, ItemData expectedItem, float durabilityCost)
    {
        if (slots == null || expectedItem == null || slotIndex < 0 || slotIndex >= slots.Count)
        {
            return InventoryDurabilityConsumeResult.Failed;
        }

        InventorySlot slot = slots[slotIndex];

        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return InventoryDurabilityConsumeResult.Failed;
        }

        if (!ItemDataComparer.AreSame(slot.Item, expectedItem))
        {
            return InventoryDurabilityConsumeResult.Failed;
        }

        if (!slot.HasDurability)
        {
            return InventoryDurabilityConsumeResult.SucceededWithoutMutation;
        }

        if (slot.IsBroken)
        {
            return InventoryDurabilityConsumeResult.Failed;
        }

        if (durabilityCost <= 0f)
        {
            return InventoryDurabilityConsumeResult.SucceededWithoutMutation;
        }

        slot.ConsumeDurability(durabilityCost);

        return InventoryDurabilityConsumeResult.Mutated;
    }

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

            slot.ConsumeDurability(durabilityCost);

            return InventoryDurabilityConsumeResult.Mutated;
        }

        return InventoryDurabilityConsumeResult.Failed;
    }
}
