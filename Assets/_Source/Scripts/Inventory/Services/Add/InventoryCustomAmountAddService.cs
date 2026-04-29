using System.Collections.Generic;
using UnityEngine;

public static class InventoryCustomAmountAddService
{
    public static bool TryAddCustomAmountItem(List<InventorySlot> slots, ItemData itemData, int count, float? currentAmountOverride, float? currentDurabilityOverride, float zeroTolerance)
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

        if (itemData.MaxAmount <= zeroTolerance)
        {
            return false;
        }

        float amountPerItem = currentAmountOverride ?? itemData.MaxAmount;

        if (amountPerItem <= zeroTolerance)
        {
            return false;
        }

        float remainingAmount = amountPerItem * count;

        FillExistingSlots(slots, itemData, ref remainingAmount, zeroTolerance);
        AddNewSlots(slots, itemData, remainingAmount, currentDurabilityOverride, zeroTolerance);

        return true;
    }

    private static void FillExistingSlots(List<InventorySlot> slots, ItemData itemData, ref float remainingAmount, float zeroTolerance)
    {
        for (int i = 0; i < slots.Count && remainingAmount > zeroTolerance; i++)
        {
            InventorySlot slot = slots[i];

            if (!CanFillSlot(slot, itemData, zeroTolerance))
            {
                continue;
            }

            float addedAmount = slot.AddAmount(remainingAmount);

            if (addedAmount > zeroTolerance)
            {
                remainingAmount -= addedAmount;
            }
        }
    }

    private static void AddNewSlots(List<InventorySlot> slots, ItemData itemData, float remainingAmount, float? currentDurabilityOverride, float zeroTolerance)
    {
        while (remainingAmount > zeroTolerance)
        {
            float amountForSlot = Mathf.Min(itemData.MaxAmount, remainingAmount);

            InventorySlot newSlot = InventorySlotFactory.Create(itemData, 1, currentDurabilityOverride, amountForSlot);

            slots.Add(newSlot);
            remainingAmount -= amountForSlot;
        }
    }

    private static bool CanFillSlot(InventorySlot slot, ItemData itemData, float zeroTolerance)
    {
        if (slot == null || slot.IsEmpty || !slot.HasAmount)
        {
            return false;
        }

        if (!ItemDataComparer.AreSame(slot.Item, itemData))
        {
            return false;
        }

        if (slot.HasDurability && slot.IsBroken)
        {
            return false;
        }

        return slot.CurrentAmount + zeroTolerance < itemData.MaxAmount;
    }
}
