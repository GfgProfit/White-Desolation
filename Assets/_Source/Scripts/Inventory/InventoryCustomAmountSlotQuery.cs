using System.Collections.Generic;

public static class InventoryCustomAmountSlotQuery
{
    public static bool TryFindFirstMatchingSlotIndex(IReadOnlyList<InventorySlot> slots, ItemData itemData, float requiredAmount, float zeroTolerance, out int slotIndex)
    {
        slotIndex = -1;

        if (slots == null || itemData == null || requiredAmount <= 0f)
        {
            return false;
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

            if (!slot.HasAmount)
            {
                continue;
            }

            if (slot.CurrentAmount + zeroTolerance < requiredAmount)
            {
                continue;
            }

            slotIndex = i;
            return true;
        }

        return false;
    }
}