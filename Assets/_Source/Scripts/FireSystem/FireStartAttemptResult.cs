public readonly struct FireStartAttemptResult
{
    public FireStartAttemptStatus Status { get; }
    public bool Success { get; }
    public float TargetFill { get; }

    public bool Started => Status == FireStartAttemptStatus.Started;
    public bool ShouldRebuildAvailableItems => Status == FireStartAttemptStatus.FailedToPayAttemptCost;

    private FireStartAttemptResult(FireStartAttemptStatus status, bool success, float targetFill)
    {
        Status = status;
        Success = success;
        TargetFill = targetFill;
    }

    public static FireStartAttemptResult MissingRequiredItems()
    {
        return new FireStartAttemptResult(FireStartAttemptStatus.MissingRequiredItems, false, 0f);
    }

    public static FireStartAttemptResult FailedToPayAttemptCost()
    {
        return new FireStartAttemptResult(FireStartAttemptStatus.FailedToPayAttemptCost, false, 0f);
    }

    public static FireStartAttemptResult Start(bool success, float targetFill)
    {
        return new FireStartAttemptResult(FireStartAttemptStatus.Started, success, targetFill);
    }
}