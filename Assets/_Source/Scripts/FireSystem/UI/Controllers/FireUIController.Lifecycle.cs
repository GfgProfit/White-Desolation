using UnityEngine;

public partial class FireUIController
{
    private void Awake()
    {
        _controlLockSession = new FireUIControlLockSession(this, _disableWhileOpen, _objectsDisableWhileOpen);
        _startWindowPresenter = new FireStartWindowPresenter(_startRoot, _igniterView, _tinderView, _fuelView, _accelerantView, _baseChanceText, _successChanceText, _burnTimeText, _startButton, _closeButton);
        _burningWindowPresenter = new FireBurningOperationWindowPresenter(_burningWindowView);
        _availableItemService = new FireStartAvailableItemService(_inventory, AccelerantAmountCost);
        _attemptService = new FireStartAttemptService(_inventory, _failedMinFill, _failedMaxFill);
        _completionService = new FireStartCompletionService(_inventory);
        _burningOperationService = new FireBurningOperationService(_inventory);

        _startWindowPresenter.Bind(PreviousIgniter, NextIgniter, PreviousTinder, NextTinder, PreviousFuel, NextFuel, PreviousAccelerant, NextAccelerant, StartFireAttempt, CloseAll);
        _burningWindowPresenter.Bind(SelectAddFuelTab, SelectCookingTab, SelectWaterTab, ExecuteBurningAction, CloseAll, DecreaseBurningWaterAmount, IncreaseBurningWaterAmount);

        _startWindowPresenter.Hide();
        _burningWindowPresenter.Hide();

        _progressView?.Hide();
    }

    private void OnDestroy()
    {
        _controlLockSession?.Release();
    }

    private void OnDisable()
    {
        if (_startRoutine != null)
        {
            StopCoroutine(_startRoutine);
            _startRoutine = null;
        }

        EndBurningOperationTimeAdvance();

        _startWindowPresenter?.Hide();
        _burningWindowPresenter?.Hide();
        _progressView?.Hide();

        _currentSource = null;

        _controlLockSession?.Release();
    }

    private void Update()
    {
        bool startWindowOpen = _startWindowPresenter != null && _startWindowPresenter.IsOpen;
        bool burningWindowOpen = _burningWindowPresenter != null && _burningWindowPresenter.IsOpen;

        if (!startWindowOpen && !burningWindowOpen)
        {
            return;
        }

        if (burningWindowOpen)
        {
            RefreshBurningRuntimeState();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            CloseAll();
        }
    }
}
