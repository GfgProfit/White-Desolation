using System.Collections.Generic;

public static class InventorySeparateSlotAddService
{
    public static bool TryAddSeparateSlotItems(List<InventorySlot> slots, ItemData itemData, int count, float? currentDurabilityOverride, float? currentAmountOverride, float? currentHydrationOverride = null, float? currentCaloriesOverride = null)
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

        for (int i = 0; i < count; i++)
        {
            InventorySlot newSlot = InventorySlotFactory.Create(itemData, 1, currentDurabilityOverride, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride);

            slots.Add(newSlot);
        }

        return true;
    }
}
