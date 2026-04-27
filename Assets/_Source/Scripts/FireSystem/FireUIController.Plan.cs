public partial class FireUIController
{
    private FireStartPlan BuildCurrentPlan()
    {
        ItemData igniter = _selectionState.GetIgniter(_availableIgniters);
        ItemData tinder = _selectionState.GetTinder(_availableTinders);
        ItemData fuel = _selectionState.GetFuel(_availableFuels);
        ItemData accelerant = _selectionState.GetAccelerant(_availableAccelerants);

        return FireStartPlanBuilder.Build(_config, igniter, tinder, fuel, accelerant, _defaultStartDurationSeconds, _accelerantStartDurationSeconds, AccelerantAmountCost);
    }
}