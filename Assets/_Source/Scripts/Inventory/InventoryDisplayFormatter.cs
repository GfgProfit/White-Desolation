using UnityEngine;

public static class InventoryDisplayFormatter
{
    public static string FormatCellCount(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return string.Empty;

        if (slot.HasAmount)
            return FormatAmount(slot.Item.AmountUnit, slot.CurrentAmount);

        if (slot.Count > 1)
            return $"x{slot.Count}";

        return string.Empty;
    }

    public static string FormatPrimaryValue(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return string.Empty;

        if (slot.HasAmount)
            return $"{FormatAmount(slot.Item.AmountUnit, slot.CurrentAmount)} / {FormatAmount(slot.Item.AmountUnit, slot.Item.MaxAmount)}";

        return $"x{slot.Count}";
    }

    public static bool TryGetDurabilityText(InventorySlot slot, out string text)
    {
        text = string.Empty;

        if (slot == null || slot.Item == null || !slot.Item.UsesDurability)
            return false;

        if (slot.Item.IsUnbreakable)
        {
            text = "100%";
            return true;
        }

        int percent = Mathf.RoundToInt(slot.Durability01 * 100f);
        text = $"{percent}%";
        return true;
    }

    public static bool TryGetWeightText(InventorySlot slot, out string text)
    {
        text = string.Empty;

        if (slot == null || slot.Item == null)
            return false;

        float currentWeight = InventoryWeightCalculator.GetSlotWeightKg(slot);
        if (currentWeight <= 0f)
            return false;

        text = $"{currentWeight:0.##} кг";
        return true;
    }

    public static bool TryGetCaloriesText(InventorySlot slot, out string text)
    {
        text = string.Empty;

        if (slot == null || slot.Item == null)
            return false;

        if (slot.Item.RestoreCalories == 0)
            return false;

        text = FormatSignedInt(slot.Item.RestoreCalories);
        return true;
    }

    public static bool TryGetHydrationText(InventorySlot slot, out string text)
    {
        text = string.Empty;

        if (slot == null || slot.Item == null)
            return false;

        if (Mathf.Approximately(slot.Item.RestoreHydration, 0f))
            return false;

        text = FormatSignedFloat(slot.Item.RestoreHydration);
        return true;
    }

    public static string FormatPrimaryActionLabel(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return "Использовать";

        return slot.Item.PrimaryAction switch
        {
            ItemPrimaryActionType.Use => "Использовать",
            ItemPrimaryActionType.Action => "Действие",
            _ => "Недоступно"
        };
    }

    public static string FormatDurabilityShort(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || !slot.Item.UsesDurability)
            return string.Empty;

        if (slot.Item.IsUnbreakable)
            return "100%";

        int percent = Mathf.RoundToInt(slot.Durability01 * 100f);
        return $"{percent}%";
    }

    public static string FormatCarryWeight(float currentWeightKg, float maxWeightKg)
    {
        string color = currentWeightKg > maxWeightKg ? "<color=#9E2F3C>" : "<color=white>";

        if (maxWeightKg > 0f)
            return $"{currentWeightKg:0.##} / {color}{maxWeightKg:0.##}</color> кг";

        return $"{currentWeightKg:0.##} кг";
    }

    public static string FormatAmount(ItemAmountUnit unit, float value)
    {
        string suffix = unit switch
        {
            ItemAmountUnit.Liter => "л",
            ItemAmountUnit.Kilogram => "кг",
            _ => "ед."
        };

        return $"{value:0.##} {suffix}";
    }

    private static string FormatSignedFloat(float value)
    {
        if (value > 0f)
            return $"+{value:0.##}";

        if (value < 0f)
            return $"{value:0.##}";

        return "0";
    }

    private static string FormatSignedInt(int value)
    {
        if (value > 0)
            return $"+{value}";

        if (value < 0)
            return value.ToString();

        return "0";
    }
}