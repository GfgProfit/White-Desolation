public readonly struct InventoryUseCompletionResult
{
    public bool Success { get; }
    public bool HasNextPlan { get; }
    public ItemUsePlan NextPlan { get; }

    private InventoryUseCompletionResult(bool success, bool hasNextPlan, ItemUsePlan nextPlan)
    {
        Success = success;
        HasNextPlan = hasNextPlan;
        NextPlan = nextPlan;
    }

    public static InventoryUseCompletionResult Failed() => new(false, false, null);
    public static InventoryUseCompletionResult Completed() => new(true, false, null);
    public static InventoryUseCompletionResult ContinueWith(ItemUsePlan nextPlan) => new(true, nextPlan != null, nextPlan);
}