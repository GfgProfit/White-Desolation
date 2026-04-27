public static class InventoryCarryWeightPresenter
{
    public static InventoryCarryWeightViewModel Build(InventoryController inventoryController)
    {
        if (inventoryController == null)
        {
            return new InventoryCarryWeightViewModel(string.Empty, string.Empty, 0f, 0f);
        }

        float currentWeightKg = inventoryController.CurrentCarryWeightKg;
        float maxWeightKg = inventoryController.MaxCarryWeightKg;

        return new InventoryCarryWeightViewModel(InventoryDisplayFormatter.FormatCarryWeight(currentWeightKg, maxWeightKg), InventoryDisplayFormatter.FormatCarryWeight(currentWeightKg, 0f), maxWeightKg, currentWeightKg);
    }
}