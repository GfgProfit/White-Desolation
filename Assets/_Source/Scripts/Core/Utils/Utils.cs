using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Utils
{
    public static string ToHexRGB(Color color)
    {
        byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255);
        byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255);
        byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static string ToHexRGBA(Color color)
    {
        byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255);
        byte g = (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255);
        byte b = (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255);
        byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(color.a * 255f), 0, 255);

        return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
    }

    public static string ToHexRGB(Color32 color) => $"#{color.r:X2}{color.g:X2}{color.b:X2}";

    public static string ToHexRGBA(Color32 color) => $"#{color.r:X2}{color.g:X2}{color.b:X2}{color.a:X2}";

    public static bool TryParseHexColor(string hex, out Color color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(hex))
        {
            return false;
        }

        return ColorUtility.TryParseHtmlString(hex, out color);
    }

    public static Color ParseHexColor(string hex)
    {
        if (TryParseHexColor(hex, out var color))
        {
            return color;
        }

        throw new FormatException($"Invalid hex color: '{hex}'");
    }

    public static string FormatNumber(long value, char separator)
    {
        if (value < 1000)
        {
            return value.ToString();
        }

        List<char> chars = new();

        string stringValue = value.ToString();
        int offset = 0;

        for (int i = stringValue.Length - 1; i >= 0; i--)
        {
            chars.Add(stringValue[i]);

            if ((chars.Count - offset) % 3 == 0)
            {
                chars.Add(separator);
                offset++;
            }
        }

        chars.Reverse();

        return string.Join("", chars.ToArray()).Trim(separator);
    }

    public static void SetDurabilityColor(WorldItem worldItem, TMP_Text durabilityText, Image durabilityIcon = null)
    {
        if (worldItem == null || worldItem.ItemData == null)
        {
            return;
        }

        float durabilityPercent = worldItem.ItemData.IsUnbreakable ? 100f : Mathf.Clamp01(worldItem.CurrentDurability / Mathf.Max(0.0001f, worldItem.ItemData.MaxDurability)) * 100f;

        SetDurability(durabilityPercent, durabilityText, durabilityIcon);
    }

    public static void SetDurabilityColor(InventorySlot slot, TMP_Text durabilityText, Image durabilityIcon = null)
    {
        if (slot == null)
        {
            return;
        }

        SetDurability(slot.Durability01 * 100f, durabilityText, durabilityIcon);
    }

    private static void SetDurability(float durabilityPercent, TMP_Text durabilityText, Image durabilityIcon)
    {
        if (durabilityPercent >= 66)
        {
            if (durabilityText != null)
            {
                durabilityText.color = Color.white;
            }

            if (durabilityIcon != null)
            {
                durabilityIcon.color = ParseHexColor("#61766F");
            }
        }
        else if (durabilityPercent >= 33 && durabilityPercent < 66)
        {
            Color a = ParseHexColor("#D7A14C");

            if (durabilityText != null)
            {
                durabilityText.color = a;
            }

            if (durabilityIcon != null)
            {
                durabilityIcon.color = a;
            }
        }
        else
        {
            Color a = ParseHexColor("#9E2F3C");

            if (durabilityText != null)
            {
                durabilityText.color = a;
            }

            if (durabilityIcon != null)
            {
                durabilityIcon.color = a;
            }
        }
    }
}
