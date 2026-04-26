public sealed class FireStartPlan
{
    public ItemData Igniter { get; }
    public ItemData Tinder { get; }
    public ItemData Fuel { get; }
    public ItemData Accelerant { get; }

    public bool UsesAccelerant { get; }
    public float SuccessChance { get; }
    public float BurnMinutes { get; }
    public float StartDurationSeconds { get; }

    public FireStartCost AttemptCost { get; }
    public FireStartCost SuccessCost { get; }

    public bool HasRequiredItems => Igniter != null && Tinder != null && Fuel != null;

    public FireStartPlan(ItemData igniter, ItemData tinder, ItemData fuel, ItemData accelerant, bool usesAccelerant, float successChance, float burnMinutes, float startDurationSeconds, FireStartCost attemptCost, FireStartCost successCost)
    {
        Igniter = igniter;
        Tinder = tinder;
        Fuel = fuel;
        Accelerant = accelerant;

        UsesAccelerant = usesAccelerant;
        SuccessChance = successChance;
        BurnMinutes = burnMinutes;
        StartDurationSeconds = startDurationSeconds;

        AttemptCost = attemptCost ?? new FireStartCost();
        SuccessCost = successCost ?? new FireStartCost();
    }
}