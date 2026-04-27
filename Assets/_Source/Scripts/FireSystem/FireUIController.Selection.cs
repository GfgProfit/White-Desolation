public partial class FireUIController
{
    private void ResetSelectionIndexes()
    {
        _selectionState.Reset(_availableIgniters.Count, _availableTinders.Count, _availableFuels.Count);
    }

    private void PreviousIgniter()
    {
        _selectionState.PreviousIgniter(_availableIgniters.Count);
        RefreshAllViews();
    }

    private void NextIgniter()
    {
        _selectionState.NextIgniter(_availableIgniters.Count);
        RefreshAllViews();
    }

    private void PreviousTinder()
    {
        _selectionState.PreviousTinder(_availableTinders.Count);
        RefreshAllViews();
    }

    private void NextTinder()
    {
        _selectionState.NextTinder(_availableTinders.Count);
        RefreshAllViews();
    }

    private void PreviousFuel()
    {
        _selectionState.PreviousFuel(_availableFuels.Count);
        RefreshAllViews();
    }

    private void NextFuel()
    {
        _selectionState.NextFuel(_availableFuels.Count);
        RefreshAllViews();
    }

    private void PreviousAccelerant()
    {
        _selectionState.PreviousAccelerant(_availableAccelerants.Count);
        RefreshAllViews();
    }

    private void NextAccelerant()
    {
        _selectionState.NextAccelerant(_availableAccelerants.Count);
        RefreshAllViews();
    }
}