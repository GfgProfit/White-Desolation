using System.Collections.Generic;
using UnityEngine;

public static class InventorySlotReplacementService
{
    public static bool TryReplaceSlotItem(InventorySlot slot, ItemData newItemData)
    {
        if (newItemData == null)
        {
            return false;
        }

        if (slot == null || slot.IsEmpty)
        {
            return false;
        }

        int count = Mathf.Max(1, slot.Count);
        slot.Initialize(newItemData, count);

        return true;
    }

    public static bool TryReplaceSlotItem(IReadOnlyList<InventorySlot> slots, int slotIndex, ItemData newItemData)
    {
        if (!InventorySlotQuery.TryGetNonEmptySlot(slots, slotIndex, out InventorySlot slot))
        {
            return false;
        }

        return TryReplaceSlotItem(slot, newItemData);
    }
}