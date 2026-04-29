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

        return new InteractionInspectInfo(itemData.Icon, itemData.DisplayName, itemData.Description, FormatDurabilityText(itemData, currentDurability), HasDurability(itemData), ResolveDurabilityColor(itemData, currentDurability), FormatWeightText(currentWeightKg));
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

        int percent = Mathf.RoundToInt(GetDurability01(itemData, currentDurability) * 100f);
        return $"{percent}%";
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

        float normalized = GetDurability01(itemData, currentDurability);

        if (normalized >= 0.66f)
        {
            return Color.white;
        }
        else if (normalized >= 0.33f && normalized < 0.66f)
        {
            return Utils.ParseHexColor("#D7A14C");
        }
        else
        {
            return Utils.ParseHexColor("#9E2F3C");
        }
    }

    private static float GetDurability01(ItemData itemData, float currentDurability)
    {
        if (itemData == null || itemData.IsUnbreakable)
        {
            return 1f;
        }

        float maxDurability = Mathf.Max(0.0001f, itemData.MaxDurability);
        return Mathf.Clamp01(currentDurability / maxDurability);
    }
}
