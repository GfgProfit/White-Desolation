using UnityEngine;

public static class ItemUseAvailabilityPolicy
{
    private const float ZeroTolerance = 0.0001f;

    public static bool CanUseSlot(ItemUseContext context, InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (context.IsUsingItem)
        {
            return false;
        }

        if (slot.IsBroken)
        {
            return slot.Item.PrimaryAction == ItemPrimaryActionType.Action;
        }

        return slot.Item.PrimaryAction switch
        {
            ItemPrimaryActionType.Use => CanUseConsumableSlot(context, slot),
            ItemPrimaryActionType.Action => true,
            _ => false
        };
    }

    private static bool CanUseConsumableSlot(ItemUseContext context, InventorySlot slot)
    {
        if (slot == null || slot.Item == null || !context.HasPlayerNeeds)
        {
            return false;
        }

        if (slot.Item.RequiresOpening)
        {
            return CanUseClosedConsumableSlot(context, slot);
        }

        if (ItemVolumeDrinkPolicy.IsVolumeDrink(slot))
        {
            return slot.CurrentAmount > ZeroTolerance && context.PlayerNeeds.MissingThirst > ZeroTolerance;
        }

        bool hasHydrationEffect = Mathf.Abs(slot.CurrentHydration) > ZeroTolerance;
        bool hasCaloriesEffect = Mathf.Abs(slot.CurrentCalories) > ZeroTolerance;

        return hasHydrationEffect || hasCaloriesEffect;
    }

    private static bool CanUseClosedConsumableSlot(ItemUseContext context, InventorySlot slot)
    {
        if (slot == null || slot.Item == null || !context.HasInventory)
        {
            return false;
        }

        ItemData item = slot.Item;

        if (!item.RequiresOpening)
        {
            return false;
        }

        if (item.AfterOpen == null || item.AfterOpen == item || item.AfterOpen.RequiresOpening)
        {
            return false;
        }

        if (!context.Inventory.ContainsUsableItem(item.NeedsToOpen))
        {
            return false;
        }

        InventorySlot previewOpenedSlot = new();
        previewOpenedSlot.Initialize(item.AfterOpen, 1);

        return CanUseConsumableSlot(context, previewOpenedSlot);
    }
}
