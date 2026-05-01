using UnityEngine;

public static class InventoryDurabilityFormatter
{
    public static bool TryFormat(InventorySlot slot, out string text)
    {
        text = string.Empty;

        if (slot == null || slot.Item == null || !slot.Item.UsesDurability)
        {
            return false;
        }

        text = FormatShort(slot);
        return true;
    }

    public static string FormatShort(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || !slot.Item.UsesDurability)
        {
            return string.Empty;
        }

        if (slot.Item.IsUnbreakable)
        {
            return "100%";
        }

        int percent = Mathf.RoundToInt(slot.Durability01 * 100f);
        return $"{percent}%";
    }
}
