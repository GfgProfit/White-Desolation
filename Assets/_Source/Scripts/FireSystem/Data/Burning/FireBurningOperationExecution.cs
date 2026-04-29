public sealed class FireBurningOperationExecution
{
    public FireBurningOperationType Type { get; }
    public float GameMinutes { get; }
    public ItemData ResultItem { get; }
    public int ResultCount { get; }
    public float? ResultAmountOverride { get; }
    public float? ResultDurabilityOverride { get; }

    public FireBurningOperationExecution(FireBurningOperationType type, float gameMinutes, ItemData resultItem, int resultCount = 1, float? resultAmountOverride = null, float? resultDurabilityOverride = null)
    {
        Type = type;
        GameMinutes = gameMinutes;
        ResultItem = resultItem;
        ResultCount = resultCount;
        ResultAmountOverride = resultAmountOverride;
        ResultDurabilityOverride = resultDurabilityOverride;
    }
}
