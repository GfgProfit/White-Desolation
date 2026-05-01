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

        ShowInventoryWindowTab(false);
        RefreshView();
    }

    private void Close()
    {
        if (_windowState == null)
        {
            return;
        }

        if (_useRoutineState.IsUsingItem || IsCrafting)
        {
            return;
        }

        _windowState.Close();
    }
}
