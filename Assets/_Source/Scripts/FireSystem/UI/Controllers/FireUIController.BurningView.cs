public partial class FireUIController
{
    private void RefreshBurningWindow()
    {
        if (_burningWindowPresenter == null)
        {
            return;
        }

        FireBurningOperationListBuilder.Rebuild(_burningOperationList, _burningSelectionState.Tab, _inventory, _config, _meltedWaterItem, _boiledWaterItem);
        _burningSelectionState.Clamp(_burningOperationList, BurningOperationSettings, GetSelectedMaxWaterAmount());

        FireBurningOperationPlan plan = BuildBurningOperationPlan();
        BuildBurningAmountControls(out string amountText, out bool canDecreaseAmount, out bool canIncreaseAmount);

        RefreshBurningTimeText();
        _burningWindowPresenter.RebuildList(_burningOperationList.Entries, _burningSelectionState.SelectedIndex, SelectBurningListItem, amountText, canDecreaseAmount, canIncreaseAmount);
        _burningWindowPresenter.SetAction(FireBurningDisplayFormatter.GetActionText(plan.Type), plan.CanExecute);
    }

    private FireBurningOperationPlan BuildBurningOperationPlan()
    {
        return FireBurningOperationPlanBuilder.Build(
            _burningSelectionState.Tab,
            _burningSelectionState.WaterMode,
            _currentSource,
            _inventory,
            _burningOperationList,
            _burningSelectionState.SelectedIndex,
            _burningSelectionState.SelectedWaterAmount,
            _meltedWaterItem,
            _boiledWaterItem,
            BurningOperationSettings);
    }

    private void BuildBurningAmountControls(out string amountText, out bool canDecrease, out bool canIncrease)
    {
        amountText = string.Empty;
        canDecrease = false;
        canIncrease = false;

        if (_burningSelectionState.Tab != FireBurningOperationTab.Water)
        {
            return;
        }

        float maxWaterAmount = GetSelectedMaxWaterAmount();
        canDecrease = _burningSelectionState.CanDecreaseWaterAmount(BurningOperationSettings, maxWaterAmount);
        canIncrease = _burningSelectionState.CanIncreaseWaterAmount(maxWaterAmount);
        amountText = $"{FireBurningDisplayFormatter.FormatLiters(_burningSelectionState.SelectedWaterAmount)} л";
    }

    private void RefreshBurningTimeText()
    {
        float remainingMinutes = _currentSource != null ? _currentSource.RemainingBurnMinutes : 0f;
        _burningWindowPresenter.SetBurningTime(FireBurningDisplayFormatter.BuildRemainingBurnTimeText(remainingMinutes));
    }

    private float GetSelectedMaxWaterAmount()
    {
        return GetMaxWaterAmount(_burningSelectionState.WaterMode);
    }

    private float GetMaxWaterAmount(FireBurningWaterMode waterMode)
    {
        if (_burningOperationService == null)
        {
            return 0f;
        }

        return _burningOperationService.GetMaxWaterAmount(waterMode, _meltedWaterItem, BurningOperationSettings);
    }
}
