public partial class FireUIController
{
    private void RebuildAvailableItems()
    {
        _availableItemService.Rebuild(_config, _availableIgniters, _availableTinders, _availableFuels, _availableAccelerants);
    }
}