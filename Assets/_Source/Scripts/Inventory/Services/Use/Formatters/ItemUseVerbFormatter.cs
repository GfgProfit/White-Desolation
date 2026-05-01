public static class ItemUseVerbFormatter
{
    public const string OpeningVerb = "открывает";

    public static string ResolveUseVerb(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return "использует";
        }

        if (slot.Item.Category == ItemCategory.Water)
        {
            return "пьет";
        }

        if (slot.Item.Category == ItemCategory.Food)
        {
            return "ест";
        }

        if (slot.Item.Category == ItemCategory.Resource)
        {
            return "собирает";
        }

        if (slot.Item.Category == ItemCategory.Tool)
        {
            return "ремонтирует";
        }

        return OpeningVerb;
    }
}
