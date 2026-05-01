public static class InventoryPrimaryActionLabelFormatter
{
    private const string UseLabel = "Использовать";
    private const string ActionLabel = "Действие";
    private const string BrokenLabel = "Сломан";
    private const string EatLabel = "Съесть";
    private const string DrinkLabel = "Выпить";
    private const string OpenLabel = "Открыть";
    private const string UnavailableLabel = "Недоступно";

    public static string Format(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return UseLabel;
        }

        if (slot.IsBroken)
        {
            return slot.Item.PrimaryAction == ItemPrimaryActionType.Action ? ActionLabel : BrokenLabel;
        }

        if (slot.Item.Category == ItemCategory.Food && !slot.Item.RequiresOpening)
        {
            return EatLabel;
        }

        if (slot.Item.Category == ItemCategory.Water && slot.Item.PrimaryAction == ItemPrimaryActionType.Use)
        {
            return DrinkLabel;
        }

        if (slot.Item.RequiresOpening)
        {
            return OpenLabel;
        }

        return slot.Item.PrimaryAction switch
        {
            ItemPrimaryActionType.Use => UseLabel,
            ItemPrimaryActionType.Action => ActionLabel,
            _ => UnavailableLabel
        };
    }
}
