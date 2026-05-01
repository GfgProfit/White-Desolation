public static class InventorySignedValueFormatter
{
    public static string FormatFloat(float value)
    {
        if (value > 0f)
        {
            return $"+{value:0.##}";
        }

        if (value < 0f)
        {
            return $"{value:0.##}";
        }

        return "0";
    }

    public static string FormatInt(int value)
    {
        if (value > 0)
        {
            return $"+{value}";
        }

        if (value < 0)
        {
            return value.ToString();
        }

        return "0";
    }
}
