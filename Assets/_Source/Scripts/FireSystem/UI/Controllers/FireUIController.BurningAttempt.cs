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

        if (_currentSource == null || !_currentSource.IsBurning)
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
        float duration = Mathf.Max(0.01f, BurningActionDurationSeconds);
        float elapsed = 0f;
        float advancedGameMinutes = 0f;

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
                _currentSource?.ConsumeBurnTime(deltaGameMinutes);
                advancedGameMinutes = targetAdvancedGameMinutes;
            }

            _progressView?.SetFill(normalized);
            RefreshBurningTimeText();

            yield return null;
        }

        _progressView?.SetFill(1f);

        _burningOperationService.Complete(execution);

        _startRoutine = null;

        _progressView?.Hide();
        RefreshBurningWindow();
    }
}
