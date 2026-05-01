public static class InventoryAmountFormatter
{
    public static string Format(ItemAmountUnit unit, float value)
    {
        string suffix = unit switch
        {
            ItemAmountUnit.Liter => "л",
            ItemAmountUnit.Kilogram => "кг",
            _ => "ед."
        };

        return $"{value:0.##} {suffix}";
    }
}
