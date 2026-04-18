using UnityEngine;

public static class InventoryWeightCalculator
{
    private const float ZeroTolerance = 0.0001f;

    public static float GetSlotWeightKg(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return 0f;

        ItemData item = slot.Item;

        // Канистры, бутылки и прочие контейнеры с amount
        if (item.UsesCustomAmount && item.WeightDependsOnAmount)
        {
            return item.BaseWeightKg + (slot.CurrentAmount * item.WeightPerUnit);
        }

        float singleItemWeight = item.BaseWeightKg;

        // Еда/напитки без UsesCustomAmount:
        // вес зависит от того, сколько consumable-содержимого осталось
        if (ShouldScaleWeightByConsumableState(item))
        {
            singleItemWeight *= slot.ConsumableFill01;
        }

        return singleItemWeight * Mathf.Max(1, slot.Count);
    }

    public static float CalculateIncomingWeightKg(
        ItemData itemData,
        int count,
        float? currentAmountOverride = null,
        float? currentHydrationOverride = null,
        float? currentCaloriesOverride = null)
    {
        if (itemData == null || count <= 0)
            return 0f;

        if (itemData.UsesCustomAmount)
        {
            float amountPerItem = currentAmountOverride ?? itemData.MaxAmount;
            float totalWeight = 0f;

            for (int i = 0; i < count; i++)
            {
                float remainingAmount = amountPerItem;

                while (remainingAmount > ZeroTolerance)
                {
                    float amountForContainer = Mathf.Min(itemData.MaxAmount, remainingAmount);
                    totalWeight += CalculateSingleContainerWeightKg(itemData, amountForContainer);
                    remainingAmount -= amountForContainer;
                }
            }

            return totalWeight;
        }

        if (ShouldScaleWeightByConsumableState(itemData))
        {
            float fill01 = CalculateConsumableFill01(
                itemData,
                currentHydrationOverride ?? itemData.RestoreHydration,
                currentCaloriesOverride ?? itemData.RestoreCalories);

            return itemData.BaseWeightKg * fill01 * count;
        }

        return itemData.BaseWeightKg * count;
    }

    private static float CalculateSingleContainerWeightKg(ItemData itemData, float currentAmount)
    {
        if (itemData == null)
            return 0f;

        if (itemData.UsesCustomAmount && itemData.WeightDependsOnAmount)
        {
            return itemData.BaseWeightKg + (currentAmount * itemData.WeightPerUnit);
        }

        return itemData.BaseWeightKg;
    }

    private static bool ShouldScaleWeightByConsumableState(ItemData itemData)
    {
        if (itemData == null)
            return false;

        if (itemData.UsesCustomAmount)
            return false;

        return itemData.RestoreHydration > ZeroTolerance || itemData.RestoreCalories > 0;
    }

    private static float CalculateConsumableFill01(
        ItemData itemData,
        float currentHydration,
        float currentCalories)
    {
        if (itemData == null)
            return 1f;

        float hydration01 = -1f;
        float calories01 = -1f;

        if (itemData.RestoreHydration > ZeroTolerance)
            hydration01 = Mathf.Clamp01(currentHydration / itemData.RestoreHydration);

        if (itemData.RestoreCalories > 0)
            calories01 = Mathf.Clamp01(currentCalories / itemData.RestoreCalories);

        if (hydration01 < 0f && calories01 < 0f)
            return 1f;

        return Mathf.Max(0f, hydration01, calories01);
    }
}