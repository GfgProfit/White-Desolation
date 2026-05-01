using UnityEngine;

public static class InventoryWeightTextFormatter
{
    public static bool TryFormatSlotWeight(InventorySlot slot, out string text)
    {
        text = string.Empty;

        if (slot == null || slot.Item == null)
        {
            return false;
        }

        float currentWeight = InventoryWeightCalculator.GetSlotWeightKg(slot);

        if (currentWeight <= 0f)
        {
            return false;
        }

        text = FormatWeight(currentWeight);
        return true;
    }

    public static string FormatCarryWeight(float currentWeightKg, float maxWeightKg)
    {
        if (maxWeightKg > 0f)
        {
            string currentText = $"{currentWeightKg:0.##}";
            string maxText = $"{maxWeightKg:0.##}";

            if (currentWeightKg > maxWeightKg)
            {
                currentText = $"<color=#FF5555>{currentText}</color>";
            }

            return $"{currentText} / {maxText} кг";
        }

        return $"{currentWeightKg:0.##} кг";
    }

    private static string FormatWeight(float weightKg)
    {
        if (weightKg >= 1f)
        {
            return $"{weightKg:0.##} кг";
        }

        return $"{weightKg * 1000f:0} гр";
    }
}
