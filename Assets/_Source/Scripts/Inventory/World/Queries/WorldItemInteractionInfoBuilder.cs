using UnityEngine;

public static class WorldItemInteractionInfoBuilder
{
    public static InteractionHoverInfo BuildHoverInfo(ItemData itemData, float currentDurability)
    {
        if (itemData == null)
        {
            return InteractionHoverInfo.Empty;
        }

        InteractionHoverInfo info = new()
        {
            InteractionText = itemData.DisplayName
        };

        if (IsBroken(itemData, currentDurability))
        {
            info.InfoText = "Разрушено";
        }

        return info;
    }

    public static InteractionInspectInfo BuildInspectInfo(ItemData itemData, float currentDurability, float currentWeightKg)
    {
        if (itemData == null)
        {
            return InteractionInspectInfo.Empty;
        }

        return new InteractionInspectInfo(
            itemData.Icon,
            itemData.DisplayName,
            itemData.Description,
            FormatDurabilityText(itemData, currentDurability),
            HasDurability(itemData),
            ResolveDurabilityColor(itemData, currentDurability),
            FormatWeightText(currentWeightKg));
    }

    private static bool IsBroken(ItemData itemData, float currentDurability)
    {
        if (itemData == null)
        {
            return false;
        }

        if (!itemData.UsesDurability || itemData.IsUnbreakable)
        {
            return false;
        }

        return currentDurability <= 0.0001f;
    }

    private static bool HasDurability(ItemData itemData)
    {
        return itemData != null && itemData.UsesDurability;
    }

    private static string FormatDurabilityText(ItemData itemData, float currentDurability)
    {
        if (itemData == null)
        {
            return "—";
        }

        if (!HasDurability(itemData))
        {
            return "—";
        }

        if (itemData.IsUnbreakable)
        {
            return "Неразрушаемый";
        }

        return $"{currentDurability:0.##}%";
    }

    private static string FormatWeightText(float currentWeightKg)
    {
        if (currentWeightKg >= 1f)
        {
            return $"{currentWeightKg:0.##} кг";
        }

        return $"{currentWeightKg * 1000f:0} гр";
    }

    private static Color ResolveDurabilityColor(ItemData itemData, float currentDurability)
    {
        if (itemData == null)
        {
            return Color.white;
        }

        if (!HasDurability(itemData))
        {
            return Color.white;
        }

        if (itemData.IsUnbreakable)
        {
            return Color.white;
        }

        float maxDurability = Mathf.Max(0.0001f, itemData.MaxDurability);
        float normalized = Mathf.Clamp01(currentDurability / maxDurability);

        if (normalized <= 0.0001f)
        {
            return Color.red;
        }

        if (normalized <= 0.25f)
        {
            return new Color(1f, 0.45f, 0.25f);
        }

        if (normalized <= 0.5f)
        {
            return Color.yellow;
        }

        return Color.white;
    }
}