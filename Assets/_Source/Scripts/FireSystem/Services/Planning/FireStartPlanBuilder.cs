public static class FireStartPlanBuilder
{
    public static FireStartPlan Build(FireStartingConfig config, ItemData igniter, ItemData tinder, ItemData fuel, ItemData accelerant, float defaultStartDurationSeconds, float accelerantStartDurationSeconds, float accelerantAmountCost)
    {
        bool usesAccelerant = accelerant != null;
        float successChance = FireStartChanceCalculator.Calculate(config, igniter, tinder, fuel, accelerant);
        float burnMinutes = fuel != null ? fuel.BurnMinutes : 0f;
        float duration = usesAccelerant ? accelerantStartDurationSeconds : defaultStartDurationSeconds;
        FireStartCost attemptCost = BuildAttemptCost(igniter);
        FireStartCost successCost = BuildSuccessCost(tinder, fuel, accelerant, accelerantAmountCost);

        return new FireStartPlan(igniter, tinder, fuel, accelerant, usesAccelerant, successChance, burnMinutes, duration, attemptCost, successCost);
    }

    private static FireStartCost BuildAttemptCost(ItemData igniter)
    {
        FireStartCost cost = new FireStartCost();

        if (igniter == null)
        {
            return cost;
        }

        if (FireIgniterConsumptionPolicy.TryGetDurabilityCost(igniter, out float durabilityCost))
        {
            cost.AddDurability(igniter, durabilityCost);
        }
        else
        {
            cost.AddItem(igniter, 1);
        }

        return cost;
    }

    private static FireStartCost BuildSuccessCost(ItemData tinder, ItemData fuel, ItemData accelerant, float accelerantAmountCost)
    {
        FireStartCost cost = new FireStartCost();

        if (tinder != null)
        {
            cost.AddItem(tinder, 1);
        }

        if (fuel != null)
        {
            cost.AddItem(fuel, 1);
        }

        if (accelerant == null)
        {
            return cost;
        }

        if (accelerant.UsesCustomAmount)
        {
            cost.AddCustomAmount(accelerant, accelerantAmountCost);
        }
        else
        {
            cost.AddItem(accelerant, 1);
        }

        return cost;
    }
}