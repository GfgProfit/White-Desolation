public partial class InventoryUIController
{
    private void Open()
    {
        if (_windowState == null)
        {
            return;
        }

        if (!_windowState.Open())
        {
            return;
        }

        RefreshView();
    }

    private void Close()
    {
        if (_windowState == null)
        {
            return;
        }

        if (_useRoutineState.IsUsingItem)
        {
            return;
        }

        _windowState.Close();
    }
}