public static class ItemVolumeDrinkPolicy
{
    private const float ZeroTolerance = 0.0001f;

    public static bool IsVolumeDrink(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
        {
            return false;
        }

        if (slot.Item.PrimaryAction != ItemPrimaryActionType.Use)
        {
            return false;
        }

        if (slot.Item.Category != ItemCategory.Water)
        {
            return false;
        }

        if (!slot.HasAmount)
        {
            return false;
        }

        if (slot.Item.AmountUnit != ItemAmountUnit.Liter)
        {
            return false;
        }

        if (slot.CurrentAmount <= ZeroTolerance)
        {
            return false;
        }

        if (slot.Item.RestoreCalories > 0)
        {
            return false;
        }

        if (slot.CurrentCalories > ZeroTolerance)
        {
            return false;
        }

        return true;
    }
}
