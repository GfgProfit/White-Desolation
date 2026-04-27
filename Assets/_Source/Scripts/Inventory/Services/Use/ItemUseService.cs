using UnityEngine;

public static class ItemUseService
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

    public static bool TryBuildPlan(ItemUseContext context, int slotIndex, InventorySlot slot, out ItemUsePlan plan)
    {
        plan = null;

        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (slot.Item.RequiresOpening)
        {
            return TryBuildOpenPlan(context, slotIndex, slot, out plan);
        }

        plan = new ItemUsePlan
        {
            SlotIndex = slotIndex,
            ActionType = slot.Item.PrimaryAction,
            VerbText = ResolveUseVerb(slot),
            Duration = Mathf.Max(0.01f, context.UseDurationSeconds)
        };

        switch (slot.Item.PrimaryAction)
        {
            case ItemPrimaryActionType.Use:
                return TryBuildConsumableUsePlan(context, slot, plan);

            case ItemPrimaryActionType.Action:
                return true;

            default:
                plan = null;
                return false;
        }
    }

    public static bool IsVolumeDrink(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (slot.Item.PrimaryAction != ItemPrimaryActionType.Use)
        {
            return false;
        }

        if (slot.Item.Category != ItemCategory.Water)
        {
            return false;
        }

        if (!slot.HasAmount)
        {
            return false;
        }

        if (slot.Item.AmountUnit != ItemAmountUnit.Liter)
        {
            return false;
        }

        if (slot.CurrentAmount <= ZeroTolerance)
        {
            return false;
        }

        if (slot.Item.RestoreCalories > 0)
        {
            return false;
        }

        if (slot.CurrentCalories > ZeroTolerance)
        {
            return false;
        }

        return true;
    }

    private static bool TryBuildOpenPlan(ItemUseContext context, int slotIndex, InventorySlot slot, out ItemUsePlan plan)
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
            VerbText = "открывает",
            Duration = Mathf.Max(0.01f, context.UseDurationSeconds),
            ReplaceSlotItemAfterAction = item.AfterOpen,
            AutoUseReplacedItem = true,
            ToolItemToDamage = item.NeedsToOpen,
            ToolDurabilityCost = item.NeedsToOpenDurabilityCost
        };

        return true;
    }

    private static bool TryBuildConsumableUsePlan(ItemUseContext context, InventorySlot slot, ItemUsePlan plan)
    {
        if (!context.HasPlayerNeeds || slot == null || slot.Item == null || plan == null)
        {
            return false;
        }

        if (IsVolumeDrink(slot))
        {
            float hydrationToApply = Mathf.Min( slot.CurrentAmount, context.PlayerNeeds.MissingThirst);

            if (hydrationToApply <= ZeroTolerance)
            {
                return false;
            }

            plan.HydrationToApply = hydrationToApply;
            plan.AmountToConsume = hydrationToApply;

            return true;
        }

        float useRatio = CalculateConsumableUseRatio(context, slot);

        if (useRatio <= ZeroTolerance)
        {
            return false;
        }

        if (Mathf.Abs(slot.CurrentHydration) > ZeroTolerance)
        {
            float hydrationAmount = slot.CurrentHydration * useRatio;

            plan.HydrationToApply = hydrationAmount;
            plan.HydrationStateToConsume = hydrationAmount;
        }

        if (Mathf.Abs(slot.CurrentCalories) > ZeroTolerance)
        {
            float caloriesAmount = slot.CurrentCalories * useRatio;

            plan.CaloriesToApply = caloriesAmount;
            plan.CaloriesStateToConsume = caloriesAmount;
        }

        if (slot.HasAmount && slot.CurrentAmount > ZeroTolerance)
        {
            plan.AmountToConsume = slot.CurrentAmount * useRatio;
        }

        plan.ReplaceWhenDepleted = slot.Item.AfterUse;

        return plan.HasPlayerEffect || plan.HasInventoryConsume;
    }

    private static float CalculateConsumableUseRatio(ItemUseContext context, InventorySlot slot)
    {
        if (!context.HasPlayerNeeds || slot == null || slot.Item == null)
        {
            return 0f;
        }

        float ratio = 1f;
        bool hasPositiveEffect = false;
        bool hasAnyEffect = false;

        if (Mathf.Abs(slot.CurrentHydration) > ZeroTolerance)
        {
            hasAnyEffect = true;

            if (slot.CurrentHydration > ZeroTolerance)
            {
                hasPositiveEffect = true;

                ratio = Mathf.Min(ratio, context.PlayerNeeds.MissingThirst / slot.CurrentHydration);
            }
        }

        if (Mathf.Abs(slot.CurrentCalories) > ZeroTolerance)
        {
            hasAnyEffect = true;

            if (slot.CurrentCalories > 0f)
            {
                hasPositiveEffect = true;

                ratio = Mathf.Min(ratio, context.PlayerNeeds.MissingHunger / slot.CurrentCalories);
            }
        }

        if (!hasAnyEffect)
        {
            return 0f;
        }

        if (!hasPositiveEffect)
        {
            return 1f;
        }

        return Mathf.Clamp01(ratio);
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

        if (IsVolumeDrink(slot))
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

    private static string ResolveUseVerb(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return "использует";
        }

        if (slot.Item.Category == ItemCategory.Water)
        {
            return "пьет";
        }

        if (slot.Item.Category == ItemCategory.Food)
        {
            return "ест";
        }

        if (slot.Item.Category == ItemCategory.Resource)
        {
            return "собирает";
        }

        if (slot.Item.Category == ItemCategory.Tool)
        {
            return "ремонтирует";
        }

        return "открывает";
    }
}