using System;
using System.Collections.Generic;

public static class InventoryViewQuery
{
    public static void BuildVisibleEntries(InventoryController inventoryController, InventoryCategoryFilter filter, InventorySortMode sortMode, InventorySortDirection sortDirection, List<InventoryViewEntry> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear();

        if (inventoryController == null)
        {
            return;
        }

        for (int i = 0; i < inventoryController.SlotCount; i++)
        {
            InventorySlot slot = inventoryController.GetSlotAt(i);

            if (!ShouldShowSlot(slot, filter))
            {
                continue;
            }

            results.Add(new InventoryViewEntry(i, slot));
        }

        SortEntries(results, sortMode, sortDirection);
    }

    public static void BuildVisibleEntries(IReadOnlyList<InventorySlot> slots, InventoryCategoryFilter filter, InventorySortMode sortMode, InventorySortDirection sortDirection, List<InventoryViewEntry> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear();

        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (!ShouldShowSlot(slot, filter))
            {
                continue;
            }

            results.Add(new InventoryViewEntry(i, slot));
        }

        SortEntries(results, sortMode, sortDirection);
    }

    private static void SortEntries(List<InventoryViewEntry> results, InventorySortMode sortMode, InventorySortDirection sortDirection)
    {
        if (results == null || sortMode == InventorySortMode.None || results.Count <= 1)
        {
            return;
        }

        results.Sort((left, right) => CompareEntries(left, right, sortMode, sortDirection));
    }

    private static bool ShouldShowSlot(InventorySlot slot, InventoryCategoryFilter filter)
    {
        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return false;
        }

        return IsCategoryAllowed(slot.Item.Category, filter);
    }

    private static bool IsCategoryAllowed(ItemCategory category, InventoryCategoryFilter filter)
    {
        return filter switch
        {
            InventoryCategoryFilter.All => true,
            InventoryCategoryFilter.MiscAndFuel => category == ItemCategory.Misc || category == ItemCategory.Fuel,
            InventoryCategoryFilter.Medical => category == ItemCategory.Medical,
            InventoryCategoryFilter.Clothing => category == ItemCategory.Clothing,
            InventoryCategoryFilter.FoodAndWater => category == ItemCategory.Food || category == ItemCategory.Water,
            InventoryCategoryFilter.ToolWeaponAndAmmo => category == ItemCategory.Tool || category == ItemCategory.Weapon || category == ItemCategory.Ammo,
            InventoryCategoryFilter.Resources => category == ItemCategory.Resource,
            _ => true,
        };
    }

    private static int CompareEntries(InventoryViewEntry left, InventoryViewEntry right, InventorySortMode sortMode, InventorySortDirection sortDirection)
    {
        InventorySlot leftSlot = left.Slot;
        InventorySlot rightSlot = right.Slot;

        if (leftSlot == null && rightSlot == null)
        {
            return left.SlotIndex.CompareTo(right.SlotIndex);
        }

        if (leftSlot == null)
        {
            return 1;
        }

        if (rightSlot == null)
        {
            return -1;
        }

        return sortMode switch
        {
            InventorySortMode.Name => CompareByName(leftSlot, left.SlotIndex, rightSlot, right.SlotIndex, sortDirection),
            InventorySortMode.Durability => CompareByDurability(leftSlot, left.SlotIndex, rightSlot, right.SlotIndex, sortDirection),
            InventorySortMode.Weight => CompareByWeight(leftSlot, left.SlotIndex, rightSlot, right.SlotIndex, sortDirection),
            _ => left.SlotIndex.CompareTo(right.SlotIndex),
        };
    }

    private static int CompareByName(InventorySlot leftSlot, int leftSlotIndex, InventorySlot rightSlot, int rightSlotIndex, InventorySortDirection sortDirection)
    {
        string leftName = leftSlot.Item != null ? leftSlot.Item.DisplayName : string.Empty;
        string rightName = rightSlot.Item != null ? rightSlot.Item.DisplayName : string.Empty;

        int compare = string.Compare(leftName, rightName, StringComparison.CurrentCultureIgnoreCase);

        if (sortDirection == InventorySortDirection.Descending)
        {
            compare = -compare;
        }

        if (compare != 0)
        {
            return compare;
        }

        return leftSlotIndex.CompareTo(rightSlotIndex);
    }

    private static int CompareByDurability(InventorySlot leftSlot, int leftSlotIndex, InventorySlot rightSlot, int rightSlotIndex, InventorySortDirection sortDirection)
    {
        bool leftHasDurability = leftSlot.HasDurability;
        bool rightHasDurability = rightSlot.HasDurability;

        if (leftHasDurability != rightHasDurability)
        {
            return leftHasDurability ? -1 : 1;
        }

        if (leftHasDurability && rightHasDurability)
        {
            int compare = leftSlot.Durability01.CompareTo(rightSlot.Durability01);

            if (sortDirection == InventorySortDirection.Descending)
            {
                compare = -compare;
            }

            if (compare != 0)
            {
                return compare;
            }
        }

        return CompareByNameThenSlotIndex(leftSlot, leftSlotIndex, rightSlot, rightSlotIndex);
    }

    private static int CompareByWeight(InventorySlot leftSlot, int leftSlotIndex, InventorySlot rightSlot, int rightSlotIndex, InventorySortDirection sortDirection)
    {
        float leftWeight = InventoryWeightCalculator.GetSlotWeightKg(leftSlot);
        float rightWeight = InventoryWeightCalculator.GetSlotWeightKg(rightSlot);

        int compare = leftWeight.CompareTo(rightWeight);

        if (sortDirection == InventorySortDirection.Descending)
        {
            compare = -compare;
        }

        if (compare != 0)
        {
            return compare;
        }

        return CompareByNameThenSlotIndex(leftSlot, leftSlotIndex, rightSlot, rightSlotIndex);
    }

    private static int CompareByNameThenSlotIndex(InventorySlot leftSlot, int leftSlotIndex, InventorySlot rightSlot, int rightSlotIndex)
    {
        string leftName = leftSlot.Item != null ? leftSlot.Item.DisplayName : string.Empty;
        string rightName = rightSlot.Item != null ? rightSlot.Item.DisplayName : string.Empty;

        int compare = string.Compare(leftName, rightName, StringComparison.CurrentCultureIgnoreCase);

        if (compare != 0)
        {
            return compare;
        }

        return leftSlotIndex.CompareTo(rightSlotIndex);
    }
}
