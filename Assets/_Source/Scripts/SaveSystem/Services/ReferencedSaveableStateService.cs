public sealed class ReferencedSaveableStateService
{
    private readonly InventoryController _inventoryController;
    private readonly PlayerNeedsController _playerNeedsController;
    private readonly DayNightCycle _dayNightCycle;

    public ReferencedSaveableStateService(InventoryController inventoryController, PlayerNeedsController playerNeedsController, DayNightCycle dayNightCycle)
    {
        _inventoryController = inventoryController;
        _playerNeedsController = playerNeedsController;
        _dayNightCycle = dayNightCycle;
    }

    public void Capture(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        if (_inventoryController != null)
        {
            _inventoryController.CaptureState(saveData);
        }

        if (_playerNeedsController != null)
        {
            _playerNeedsController.CaptureState(saveData);
        }

        if (_dayNightCycle != null)
        {
            _dayNightCycle.CaptureState(saveData);
        }
    }

    public void Restore(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null)
        {
            return;
        }

        if (_inventoryController != null)
        {
            _inventoryController.RestoreState(saveData, context);
        }

        if (_playerNeedsController != null)
        {
            _playerNeedsController.RestoreState(saveData, context);
        }

        if (_dayNightCycle != null)
        {
            _dayNightCycle.RestoreState(saveData, context);
        }
    }
}