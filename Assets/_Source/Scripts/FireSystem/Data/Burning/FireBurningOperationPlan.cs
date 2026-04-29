public sealed class FireBurningOperationPlan
{
    public FireBurningOperationType Type { get; }
    public bool CanExecute { get; }
    public float GameMinutes { get; }
    public int SlotIndex { get; }
    public ItemData SourceItem { get; }
    public ItemData ResultItem { get; }
    public float Amount { get; }
    public float? ResultDurabilityOverride { get; }

    public bool RequiresProgress => CanExecute && GameMinutes > 0f;

    private FireBurningOperationPlan(FireBurningOperationType type, bool canExecute, float gameMinutes, int slotIndex, ItemData sourceItem, ItemData resultItem, float amount, float? resultDurabilityOverride)
    {
        Type = type;
        CanExecute = canExecute;
        GameMinutes = gameMinutes;
        SlotIndex = slotIndex;
        SourceItem = sourceItem;
        ResultItem = resultItem;
        Amount = amount;
        ResultDurabilityOverride = resultDurabilityOverride;
    }

    public static FireBurningOperationPlan Create(FireBurningOperationType type, bool canExecute, float gameMinutes = 0f, int slotIndex = -1, ItemData sourceItem = null, ItemData resultItem = null, float amount = 0f, float? resultDurabilityOverride = null)
    {
        return new FireBurningOperationPlan(type, canExecute, gameMinutes, slotIndex, sourceItem, resultItem, amount, resultDurabilityOverride);
    }
}
