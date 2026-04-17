using System.Text;
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
            return slot.Count.ToString();

        return string.Empty;
    }

    public static string FormatPrimaryValue(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return string.Empty;

        if (slot.HasAmount)
            return $"{FormatAmount(slot.Item.AmountUnit, slot.CurrentAmount)} / {FormatAmount(slot.Item.AmountUnit, slot.Item.MaxAmount)}";

        return $"Количество: {slot.Count}";
    }

    public static string FormatStats(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return string.Empty;

        StringBuilder sb = new();

        if (slot.HasAmount)
        {
            sb.Append("Объём: ");
            sb.Append(FormatAmount(slot.Item.AmountUnit, slot.CurrentAmount));
            sb.Append(" / ");
            sb.AppendLine(FormatAmount(slot.Item.AmountUnit, slot.Item.MaxAmount));
        }

        if (slot.Item.UsesDurability)
        {
            if (slot.Item.IsUnbreakable)
            {
                sb.AppendLine("Прочность: 100% (не ломается)");
            }
            else
            {
                sb.Append("Прочность: ");
                sb.AppendLine(FormatDurabilityShort(slot));
            }
        }

        if (!Mathf.Approximately(slot.Item.RestoreHydration, 0f))
        {
            sb.Append("Изменение жажды: ");
            sb.AppendLine(FormatSignedFloat(slot.Item.RestoreHydration));
        }

        if (slot.Item.RestoreCalories != 0)
        {
            sb.Append("Изменение калорий: ");
            sb.AppendLine(FormatSignedInt(slot.Item.RestoreCalories));
        }

        return sb.ToString().TrimEnd();
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

    private static string FormatCategory(ItemCategory category)
    {
        return category switch
        {
            ItemCategory.Food => "Еда",
            ItemCategory.Water => "Вода",
            ItemCategory.Medical => "Медикаменты",
            ItemCategory.Consumable => "Расходник",
            ItemCategory.Resource => "Ресурс",
            ItemCategory.Tool => "Инструмент",
            ItemCategory.Weapon => "Оружие",
            ItemCategory.Clothing => "Одежда",
            ItemCategory.Fuel => "Топливо",
            ItemCategory.Ammo => "Боеприпасы",
            ItemCategory.Misc => "Разное",
            _ => "Не задан"
        };
    }
}