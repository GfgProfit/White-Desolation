using UnityEngine;

public sealed class FireStartCompletionService
{
    private readonly InventoryController _inventory;

    public FireStartCompletionService(InventoryController inventory)
    {
        _inventory = inventory;
    }

    public void Complete(FireStartPlan plan, FireSourceInteractable source, bool success)
    {
        if (!success)
        {
            return;
        }

        if (plan == null)
        {
            return;
        }

        if (source == null)
        {
            Debug.LogWarning("[FireStarting] Cannot complete successful attempt: fire source is missing.");
            return;
        }

        bool consumed = FireStartCostConsumer.TryPay(_inventory, plan.SuccessCost);

        if (consumed)
        {
            source.Ignite(plan.BurnMinutes);
            return;
        }

        Debug.LogWarning("[FireStarting] Успех выпал, но не удалось потратить предметы для костра/печки.");
    }
}
