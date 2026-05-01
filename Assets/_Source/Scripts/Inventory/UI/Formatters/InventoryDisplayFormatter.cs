using UnityEngine;

public static class InventoryDisplayFormatter
{
    public static string FormatCellCount(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return string.Empty;
        }

        if (slot.HasAmount)
        {
            return InventoryAmountFormatter.Format(slot.Item.AmountUnit, slot.CurrentAmount);
        }

        if (slot.Count > 1)
        {
            return $"x{slot.Count}";
        }

        return string.Empty;
    }

    public static string FormatPrimaryValue(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return string.Empty;
        }

        if (slot.HasAmount)
        {
            string current = InventoryAmountFormatter.Format(slot.Item.AmountUnit, slot.CurrentAmount);
            string max = InventoryAmountFormatter.Format(slot.Item.AmountUnit, slot.Item.MaxAmount);

            return $"{current} / {max}";
        }

        return $"x{slot.Count}";
    }

    public static bool TryGetDurabilityText(InventorySlot slot, out string text)
    {
        return InventoryDurabilityFormatter.TryFormat(slot, out text);
    }

    public static bool TryGetWeightText(InventorySlot slot, out string text)
    {
        return InventoryWeightTextFormatter.TryFormatSlotWeight(slot, out text);
    }

    public static bool TryGetCaloriesText(InventorySlot slot, out string text)
    {
        text = string.Empty;

        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (Mathf.Approximately(slot.CurrentCalories, 0f))
        {
            return false;
        }

        text = InventorySignedValueFormatter.FormatInt(Mathf.RoundToInt(slot.CurrentCalories));
        return true;
    }

    public static bool TryGetHydrationText(InventorySlot slot, out string text)
    {
        text = string.Empty;

        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (ItemVolumeDrinkPolicy.IsVolumeDrink(slot))
        {
            text = InventorySignedValueFormatter.FormatFloat(slot.CurrentAmount);
            return true;
        }

        if (Mathf.Approximately(slot.CurrentHydration, 0f))
        {
            return false;
        }

        text = InventorySignedValueFormatter.FormatFloat(slot.CurrentHydration);
        return true;
    }

    public static string FormatPrimaryActionLabel(InventorySlot slot)
    {
        return InventoryPrimaryActionLabelFormatter.Format(slot);
    }

    public static string FormatDurabilityShort(InventorySlot slot)
    {
        return InventoryDurabilityFormatter.FormatShort(slot);
    }

    public static string FormatCarryWeight(float currentWeightKg, float maxWeightKg)
    {
        return InventoryWeightTextFormatter.FormatCarryWeight(currentWeightKg, maxWeightKg);
    }

    public static string FormatAmount(ItemAmountUnit unit, float value)
    {
        return InventoryAmountFormatter.Format(unit, value);
    }
}
