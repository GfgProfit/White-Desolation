public partial class FireUIController
{
    private void RefreshAllViews()
    {
        FireStartPlan plan = BuildCurrentPlan();
        _startWindowPresenter.Refresh(plan, _config, _inventory, AccelerantAmountCost);
    }
}