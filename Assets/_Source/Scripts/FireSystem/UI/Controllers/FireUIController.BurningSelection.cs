public partial class FireUIController
{
    private void ResetBurningState()
    {
        _burningSelectionState.Reset(BurningOperationSettings);
    }

    private void SelectAddFuelTab()
    {
        SelectBurningTab(FireBurningOperationTab.AddFuel);
    }

    private void SelectCookingTab()
    {
        SelectBurningTab(FireBurningOperationTab.Cooking);
    }

    private void SelectWaterTab()
    {
        SelectBurningTab(FireBurningOperationTab.Water);
    }

    private void SelectBurningTab(FireBurningOperationTab tab)
    {
        if (_startRoutine != null)
        {
            return;
        }

        _burningSelectionState.SelectTab(tab, BurningOperationSettings);

        RefreshBurningWindow();
    }

    private void SelectBurningListItem(int index)
    {
        if (_startRoutine != null)
        {
            return;
        }

        FireBurningWaterMode waterMode = index == 1 ? FireBurningWaterMode.BoilWater : FireBurningWaterMode.MeltSnow;
        float maxWaterAmount = GetMaxWaterAmount(waterMode);

        if (!_burningSelectionState.SelectListItem(index, _burningOperationList, BurningOperationSettings, maxWaterAmount))
        {
            return;
        }

        RefreshBurningWindow();
    }

    private void DecreaseBurningWaterAmount()
    {
        if (_burningSelectionState.Tab != FireBurningOperationTab.Water)
        {
            return;
        }

        _burningSelectionState.DecreaseWaterAmount(BurningOperationSettings, GetSelectedMaxWaterAmount());

        RefreshBurningWindow();
    }

    private void IncreaseBurningWaterAmount()
    {
        if (_burningSelectionState.Tab != FireBurningOperationTab.Water)
        {
            return;
        }

        _burningSelectionState.IncreaseWaterAmount(BurningOperationSettings, GetSelectedMaxWaterAmount());

        RefreshBurningWindow();
    }
}
