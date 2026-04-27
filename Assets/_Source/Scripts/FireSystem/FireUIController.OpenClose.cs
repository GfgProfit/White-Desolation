public partial class FireUIController
{
    public void OpenFireStarting(FireSourceInteractable source)
    {
        if (source == null)
        {
            return;
        }

        if (_startRoutine != null)
        {
            return;
        }

        _currentSource = source;

        RebuildAvailableItems();
        ResetSelectionIndexes();
        RefreshAllViews();

        _progressView?.Hide();

        _controlLockSession.Open();

        _startWindowPresenter.Show();
    }

    public void CloseAll()
    {
        if (_startRoutine != null)
        {
            StopCoroutine(_startRoutine);
            _startRoutine = null;
        }

        _startWindowPresenter.Hide();

        _progressView?.Hide();

        _controlLockSession?.Close();

        _currentSource = null;
    }
}