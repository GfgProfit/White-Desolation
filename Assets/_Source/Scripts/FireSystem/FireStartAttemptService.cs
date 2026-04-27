using UnityEngine;

public sealed class FireStartAttemptService
{
    private readonly InventoryController _inventory;
    private readonly float _failedMinFill;
    private readonly float _failedMaxFill;

    public FireStartAttemptService(InventoryController inventory, float failedMinFill, float failedMaxFill)
    {
        _inventory = inventory;
        _failedMinFill = failedMinFill;
        _failedMaxFill = failedMaxFill;
    }

    public FireStartAttemptResult Begin(FireStartPlan plan)
    {
        if (plan == null || !plan.HasRequiredItems)
        {
            return FireStartAttemptResult.MissingRequiredItems();
        }

        if (!FireStartCostConsumer.TryPay(_inventory, plan.AttemptCost))
        {
            return FireStartAttemptResult.FailedToPayAttemptCost();
        }

        bool success = plan.UsesAccelerant || Random.value <= plan.SuccessChance / 100f;
        float maxFailedFill = Mathf.Max(_failedMinFill, _failedMaxFill);
        float targetFill = success ? 1f : Random.Range(_failedMinFill, maxFailedFill);

        return FireStartAttemptResult.Start(success, targetFill);
    }
}