using UnityEngine;

public static class ItemUsePlanBuilder
{
    public static bool TryBuildPlan(ItemUseContext context, int slotIndex, InventorySlot slot, out ItemUsePlan plan)
    {
        plan = null;

        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (slot.Item.RequiresOpening)
        {
            return ItemOpeningUsePlanBuilder.TryBuildPlan(context, slotIndex, slot, out plan);
        }

        plan = new ItemUsePlan
        {
            SlotIndex = slotIndex,
            ActionType = slot.Item.PrimaryAction,
            VerbText = ItemUseVerbFormatter.ResolveUseVerb(slot),
            Duration = Mathf.Max(0.01f, context.UseDurationSeconds)
        };

        switch (slot.Item.PrimaryAction)
        {
            case ItemPrimaryActionType.Use:
                return ItemConsumableUsePlanBuilder.TryApply(context, slot, plan);

            case ItemPrimaryActionType.Action:
                return true;

            default:
                plan = null;
                return false;
        }
    }
}
