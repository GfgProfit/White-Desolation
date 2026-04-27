public partial class FireUIController
{
    private void StartFireAttempt()
    {
        if (_startRoutine != null)
        {
            return;
        }

        FireStartPlan plan = BuildCurrentPlan();
        FireStartAttemptResult result = _attemptService.Begin(plan);

        if (!result.Started)
        {
            HandleFailedStartAttempt(result);
            return;
        }

        _startWindowPresenter.Hide();

        _progressView?.Show("разводим огонь");

        _startRoutine = StartCoroutine(FireProgressRoutine(plan, result.Success, result.TargetFill));
    }

    private void HandleFailedStartAttempt(FireStartAttemptResult result)
    {
        if (result.Status == FireStartAttemptStatus.MissingRequiredItems)
        {
            RefreshAllViews();
            return;
        }

        if (result.Status == FireStartAttemptStatus.FailedToPayAttemptCost)
        {
            RebuildAvailableItems();
            ResetSelectionIndexes();
            RefreshAllViews();
        }
    }
}