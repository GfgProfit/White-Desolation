using UnityEngine;

public static class ItemOpeningUsePlanBuilder
{
    public static bool TryBuildPlan(ItemUseContext context, int slotIndex, InventorySlot slot, out ItemUsePlan plan)
    {
        plan = null;

        if (!context.HasInventory || slot == null || slot.Item == null)
        {
            return false;
        }

        ItemData item = slot.Item;

        if (!item.RequiresOpening || item.AfterOpen == null)
        {
            return false;
        }

        if (!context.Inventory.ContainsUsableItem(item.NeedsToOpen))
        {
            return false;
        }

        plan = new ItemUsePlan
        {
            SlotIndex = slotIndex,
            ActionType = ItemPrimaryActionType.Action,
            VerbText = ItemUseVerbFormatter.OpeningVerb,
            Duration = Mathf.Max(0.01f, context.UseDurationSeconds),
            ReplaceSlotItemAfterAction = item.AfterOpen,
            AutoUseReplacedItem = true,
            ToolItemToDamage = item.NeedsToOpen,
            ToolDurabilityCost = item.NeedsToOpenDurabilityCost
        };

        return true;
    }
}
