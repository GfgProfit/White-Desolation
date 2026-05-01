using UnityEngine;

public static class ItemConsumableUsePlanBuilder
{
    private const float ZeroTolerance = 0.0001f;

    public static bool TryApply(ItemUseContext context, InventorySlot slot, ItemUsePlan plan)
    {
        if (!context.HasPlayerNeeds || slot == null || slot.Item == null || plan == null)
        {
            return false;
        }

        if (ItemVolumeDrinkPolicy.IsVolumeDrink(slot))
        {
            float hydrationToApply = Mathf.Min(slot.CurrentAmount, context.PlayerNeeds.MissingThirst);

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

        ApplyHydration(slot, plan, useRatio);
        ApplyCalories(slot, plan, useRatio);
        ApplyAmount(slot, plan, useRatio);

        plan.ReplaceWhenDepleted = slot.Item.AfterUse;

        return plan.HasPlayerEffect || plan.HasInventoryConsume;
    }

    private static void ApplyHydration(InventorySlot slot, ItemUsePlan plan, float useRatio)
    {
        if (Mathf.Abs(slot.CurrentHydration) <= ZeroTolerance)
        {
            return;
        }

        float hydrationAmount = slot.CurrentHydration * useRatio;

        plan.HydrationToApply = hydrationAmount;
        plan.HydrationStateToConsume = hydrationAmount;
    }

    private static void ApplyCalories(InventorySlot slot, ItemUsePlan plan, float useRatio)
    {
        if (Mathf.Abs(slot.CurrentCalories) <= ZeroTolerance)
        {
            return;
        }

        float caloriesAmount = slot.CurrentCalories * useRatio;

        plan.CaloriesToApply = caloriesAmount;
        plan.CaloriesStateToConsume = caloriesAmount;
    }

    private static void ApplyAmount(InventorySlot slot, ItemUsePlan plan, float useRatio)
    {
        if (!slot.HasAmount || slot.CurrentAmount <= ZeroTolerance)
        {
            return;
        }

        plan.AmountToConsume = slot.CurrentAmount * useRatio;
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
}
