using System.Collections.Generic;

public static class InventoryItemQuery
{
    public static bool Contains(IReadOnlyList<InventorySlot> slots, ItemData itemData, int count = 1)
    {
        return GetTotalCount(slots, itemData) >= count;
    }

    public static bool ContainsUsableItem(IReadOnlyList<InventorySlot> slots, ItemData itemData, int count)
    {
        if (itemData == null || count <= 0)
        {
            return false;
        }

        int found = 0;

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

            if (slot.HasDurability && slot.IsBroken)
            {
                continue;
            }

            found += slot.Count;

            if (found >= count)
            {
                return true;
            }
        }

        return false;
    }

    public static int GetTotalCount(IReadOnlyList<InventorySlot> slots, ItemData itemData)
    {
        if (itemData == null)
        {
            return 0;
        }

        int total = 0;

        for (int i = 0; i < slots.Count; i++)
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

            total += slot.Count;
        }

        return total;
    }

    public static float GetTotalAmount(IReadOnlyList<InventorySlot> slots, ItemData itemData)
    {
        if (itemData == null || !itemData.UsesCustomAmount)
        {
            return 0f;
        }

        float total = 0f;

        for (int i = 0; i < slots.Count; i++)
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

            if (!slot.HasAmount)
            {
                continue;
            }

            total += slot.CurrentAmount;
        }

        return total;
    }

    public static bool HasCustomAmount(IReadOnlyList<InventorySlot> slots, ItemData itemData, float requiredAmount)
    {
        if (requiredAmount <= 0f)
        {
            return false;
        }

        return GetTotalAmount(slots, itemData) >= requiredAmount;
    }

    public static bool TryFindCustomAmountSlotIndex(IReadOnlyList<InventorySlot> slots, ItemData itemData, float requiredAmount, out int slotIndex)
    {
        slotIndex = -1;

        if (itemData == null)
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

            if (slot.CurrentAmount < requiredAmount)
            {
                continue;
            }

            slotIndex = i;
            return true;
        }

        return false;
    }
}
