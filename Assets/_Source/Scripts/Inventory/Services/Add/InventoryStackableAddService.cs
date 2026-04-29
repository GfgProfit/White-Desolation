using System.Collections.Generic;
using UnityEngine;

public static class InventoryStackableAddService
{
    public static bool TryAddStackableItems(List<InventorySlot> slots, ItemData itemData, int count, float? currentDurabilityOverride, float? currentAmountOverride)
    {
        if (slots == null)
        {
            return false;
        }

        if (itemData == null)
        {
            return false;
        }

        if (count <= 0)
        {
            return false;
        }

        int remaining = count;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot == null || slot.IsEmpty)
            {
                continue;
            }

            if (!InventoryStackMergePolicy.CanMergeIntoStack(slot, itemData, currentDurabilityOverride, currentAmountOverride))
            {
                continue;
            }

            if (slot.IsFull)
            {
                continue;
            }

            int freeSpace = slot.MaxStack - slot.Count;
            int amountToAdd = Mathf.Min(freeSpace, remaining);

            slot.AddCount(amountToAdd);
            remaining -= amountToAdd;

            if (remaining <= 0)
            {
                return true;
            }
        }

        while (remaining > 0)
        {
            int amountForNewSlot = Mathf.Min(itemData.MaxStack, remaining);

            InventorySlot newSlot = InventorySlotFactory.Create(itemData, amountForNewSlot, currentDurabilityOverride, currentAmountOverride);

            slots.Add(newSlot);

            remaining -= amountForNewSlot;
        }

        return true;
    }
}
