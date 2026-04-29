using UnityEngine;

public static class FireBurningDisplayFormatter
{
    public static string GetActionText(FireBurningOperationType type)
    {
        return type switch
        {
            FireBurningOperationType.AddFuel => "Добавить топливо",
            FireBurningOperationType.Cook => "Приготовить",
            FireBurningOperationType.MeltSnow => "Топить снег",
            FireBurningOperationType.BoilWater => "Кипятить воду",
            _ => string.Empty,
        };
    }

    public static string GetProgressText(FireBurningOperationType type)
    {
        return type switch
        {
            FireBurningOperationType.Cook => "Готовим",
            FireBurningOperationType.MeltSnow => "топим снег",
            FireBurningOperationType.BoilWater => "кипятим воду",
            _ => string.Empty,
        };
    }

    public static string BuildSlotDisplayName(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return string.Empty;
        }

        if (slot.HasAmount)
        {
            return $"{slot.Item.DisplayName} ({FormatLiters(slot.CurrentAmount)} л)";
        }

        if (slot.Count > 1)
        {
            return $"{slot.Item.DisplayName} x{slot.Count}";
        }

        return slot.Item.DisplayName;
    }

    public static string FormatLiters(float amount)
    {
        return amount.ToString("0.##");
    }

    public static string BuildRemainingBurnTimeText(float minutes)
    {
        return $"оставшееся время горения: {FormatMinutes(minutes)}";
    }

    public static string FormatMinutes(float minutes)
    {
        int totalMinutes = Mathf.CeilToInt(Mathf.Max(0f, minutes));
        int hours = totalMinutes / 60;
        int restMinutes = totalMinutes % 60;

        if (hours > 0)
        {
            return $"{hours} ч {restMinutes:00} мин";
        }

        return $"{restMinutes} мин";
    }
}
