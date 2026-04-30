using System.Collections;
using UnityEngine;

public partial class FireUIController
{
    private void ExecuteBurningAction()
    {
        if (_startRoutine != null)
        {
            return;
        }

        if (_currentSource == null)
        {
            CloseAll();
            return;
        }

        if (!_currentSource.IsBurning)
        {
            CloseAll();
            return;
        }

        FireBurningOperationPlan plan = BuildBurningOperationPlan();
        FireBurningOperationStartResult result = _burningOperationService.Begin(plan, _currentSource);

        if (!result.Succeeded)
        {
            RefreshBurningWindow();
            return;
        }

        if (result.CompletedImmediately)
        {
            RefreshBurningWindow();
            return;
        }

        BeginBurningOperation(result.Execution);
    }

    private void BeginBurningOperation(FireBurningOperationExecution execution)
    {
        if (execution == null)
        {
            RefreshBurningWindow();
            return;
        }

        _progressView?.Show(FireBurningDisplayFormatter.GetProgressText(execution.Type));
        _progressView?.SetFill(0f);

        _startRoutine = StartCoroutine(BurningOperationProgressRoutine(execution));
    }

    private IEnumerator BurningOperationProgressRoutine(FireBurningOperationExecution execution)
    {
        BeginBurningOperationTimeAdvance();

        float duration = Mathf.Max(0.01f, BurningActionDurationSeconds);
        float elapsed = 0f;
        float advancedGameMinutes = 0f;

        try
        {
            while (elapsed < duration)
            {
                float deltaTime = Mathf.Min(Time.deltaTime, duration - elapsed);
                elapsed += deltaTime;

                float normalized = Mathf.Clamp01(elapsed / duration);
                float targetAdvancedGameMinutes = execution.GameMinutes * normalized;
                float deltaGameMinutes = targetAdvancedGameMinutes - advancedGameMinutes;

                if (deltaGameMinutes > 0f)
                {
                    _gameTimeAdvancer?.AddGameMinutes(deltaGameMinutes);
                    advancedGameMinutes = targetAdvancedGameMinutes;
                }

                _progressView?.SetFill(normalized);
                RefreshBurningTimeText();

                yield return null;
            }

            _progressView?.SetFill(1f);

            _burningOperationService.Complete(execution);
        }
        finally
        {
            EndBurningOperationTimeAdvance();
        }

        _startRoutine = null;

        _progressView?.Hide();

        if (_currentSource != null && !_currentSource.IsBurning)
        {
            CloseAll();
            yield break;
        }

        RefreshBurningWindow();
    }

    private void BeginBurningOperationTimeAdvance()
    {
        if (_burningOperationAdvancingTime)
        {
            return;
        }

        _burningOperationAdvancingTime = true;

        if (_gameTimeRunController == null)
        {
            return;
        }

        _previousGameTimeRunning = _gameTimeRunController.IsRunning;
        _restoreGameTimeRunning = true;

        if (_previousGameTimeRunning)
        {
            _gameTimeRunController.SetRunning(false);
        }
    }

    private void EndBurningOperationTimeAdvance()
    {
        if (!_burningOperationAdvancingTime)
        {
            return;
        }

        _burningOperationAdvancingTime = false;

        if (_restoreGameTimeRunning && _gameTimeRunController != null)
        {
            _gameTimeRunController.SetRunning(_previousGameTimeRunning);
        }

        _restoreGameTimeRunning = false;
        _previousGameTimeRunning = false;
    }
}
