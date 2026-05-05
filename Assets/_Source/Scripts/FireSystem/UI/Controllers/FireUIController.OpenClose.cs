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

        _burningWindowPresenter?.Hide();

        _controlLockSession.Open();

        _startWindowPresenter.Show();
    }

    public void OpenBurningFire(FireSourceInteractable source)
    {
        if (source == null)
        {
            return;
        }

        if (_startRoutine != null)
        {
            return;
        }

        if (!source.IsBurning)
        {
            OpenFireStarting(source);
            return;
        }

        _currentSource = source;

        ResetBurningState();
        RefreshBurningWindow();

        _progressView?.Hide();

        _startWindowPresenter?.Hide();

        _controlLockSession.Open();

        _burningWindowPresenter.Show();
    }

    public void CloseAll()
    {
        if (_startRoutine != null)
        {
            return;
        }

        _startWindowPresenter.Hide();
        _burningWindowPresenter.Hide();

        _progressView?.Hide();

        _controlLockSession?.Close();

        _currentSource = null;
    }

    public bool TryCloseOpenWindow()
    {
        if (!IsWindowOpen)
        {
            return false;
        }

        CloseAll();
        return true;
    }
}
