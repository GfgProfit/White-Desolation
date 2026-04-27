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

        float amountPerItem = currentAmountOverride ?? itemData.MaxAmount;

        if (amountPerItem <= zeroTolerance)
        {
            return false;
        }

        for (int itemIndex = 0; itemIndex < count; itemIndex++)
        {
            float remainingAmountForItem = amountPerItem;

            while (remainingAmountForItem > zeroTolerance)
            {
                float amountForSlot = Mathf.Min(itemData.MaxAmount, remainingAmountForItem);

                InventorySlot newSlot = InventorySlotFactory.Create(itemData, 1, currentDurabilityOverride, amountForSlot);

                slots.Add(newSlot);
                remainingAmountForItem -= amountForSlot;
            }
        }

        return true;
    }
}