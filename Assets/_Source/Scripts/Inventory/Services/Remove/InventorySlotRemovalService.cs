using System.Collections.Generic;
using UnityEngine;

public static class InventorySlotRemovalService
{
    public static bool TryRemoveFromSlot(List<InventorySlot> slots, int slotIndex, int count)
    {
        if (slots == null)
        {
            return false;
        }

        if (count <= 0)
        {
            return false;
        }

        if (!InventorySlotQuery.TryGetNonEmptySlot(slots, slotIndex, out InventorySlot slot))
        {
            return false;
        }

        int amountToRemove = Mathf.Min(count, slot.Count);
        slot.RemoveCount(amountToRemove);

        if (slot.Count <= 0)
        {
            slots.RemoveAt(slotIndex);
        }

        return true;
    }
}
