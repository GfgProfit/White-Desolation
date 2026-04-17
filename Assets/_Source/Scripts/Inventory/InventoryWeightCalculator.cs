using UnityEngine;

public static class InventoryWeightCalculator
{
    public static float GetSlotWeightKg(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return 0f;

        ItemData item = slot.Item;

        if (item.UsesCustomAmount && item.WeightDependsOnAmount)
        {
            return item.BaseWeightKg + (slot.CurrentAmount * item.WeightPerUnit);
        }

        return item.BaseWeightKg * Mathf.Max(1, slot.Count);
    }

    public static float CalculateIncomingWeightKg(
        ItemData itemData,
        int count,
        float? currentAmountOverride = null)
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

                while (remainingAmount > 0.0001f)
                {
                    float amountForContainer = Mathf.Min(itemData.MaxAmount, remainingAmount);
                    totalWeight += CalculateSingleContainerWeightKg(itemData, amountForContainer);
                    remainingAmount -= amountForContainer;
                }
            }

            return totalWeight;
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
}