using System.Collections.Generic;
using UnityEngine;

public static class InventoryItemRemovalService
{
    public static bool TryRemoveItem(List<InventorySlot> slots, ItemData itemData, int count)
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

        if (InventoryItemQuery.GetTotalCount(slots, itemData) < count)
        {
            return false;
        }

        int remainingToRemove = count;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = slots[i];

            if (slot == null || slot.IsEmpty)
            {
                continue;
            }

            if (!ItemDataComparer.AreSame(slot.Item, itemData))
            {
                continue;
            }

            int amountToRemove = Mathf.Min(slot.Count, remainingToRemove);

            slot.RemoveCount(amountToRemove);
            remainingToRemove -= amountToRemove;

            if (slot.Count <= 0)
            {
                slots.RemoveAt(i);
            }

            if (remainingToRemove <= 0)
            {
                return true;
            }
        }

        return true;
    }
}
