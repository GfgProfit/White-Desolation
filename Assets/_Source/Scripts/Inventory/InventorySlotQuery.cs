using System.Collections.Generic;

public static class InventorySlotQuery
{
    public static bool IsIndexInRange(IReadOnlyList<InventorySlot> slots, int slotIndex)
    {
        return slots != null && slotIndex >= 0 && slotIndex < slots.Count;
    }

    public static InventorySlot GetSlotOrNull(IReadOnlyList<InventorySlot> slots, int slotIndex)
    {
        if (!IsIndexInRange(slots, slotIndex))
        {
            return null;
        }

        return slots[slotIndex];
    }

    public static bool TryGetNonEmptySlot(IReadOnlyList<InventorySlot> slots, int slotIndex, out InventorySlot slot)
    {
        slot = GetSlotOrNull(slots, slotIndex);
        return slot != null && !slot.IsEmpty;
    }

    public static bool TryGetNonEmptySlotWithItem(IReadOnlyList<InventorySlot> slots, int slotIndex, out InventorySlot slot)
    {
        return TryGetNonEmptySlot(slots, slotIndex, out slot) && slot.Item != null;
    }
}