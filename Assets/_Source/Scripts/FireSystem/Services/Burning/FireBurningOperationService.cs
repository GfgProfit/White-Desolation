using UnityEngine;

public sealed class FireBurningOperationService
{
    private readonly InventoryController _inventory;

    public FireBurningOperationService(InventoryController inventory)
    {
        _inventory = inventory;
    }

    public float GetMaxWaterAmount(FireBurningWaterMode mode, ItemData meltedWaterItem, FireBurningOperationSettings settings)
    {
        if (mode == FireBurningWaterMode.MeltSnow)
        {
            return settings.MeltSnowMaxLiters;
        }

        if (_inventory == null || meltedWaterItem == null)
        {
            return 0f;
        }

        return Mathf.Max(0f, _inventory.GetTotalAmount(meltedWaterItem));
    }

    public FireBurningOperationStartResult Begin(FireBurningOperationPlan plan, FireSourceInteractable source)
    {
        if (plan == null || !plan.CanExecute || source == null || !source.IsBurning || _inventory == null)
        {
            return FireBurningOperationStartResult.Failed();
        }

        return plan.Type switch
        {
            FireBurningOperationType.AddFuel => AddFuel(plan, source),
            FireBurningOperationType.Cook => StartCooking(plan, source),
            FireBurningOperationType.MeltSnow => StartMeltingSnow(plan, source),
            FireBurningOperationType.BoilWater => StartBoilingWater(plan, source),
            _ => FireBurningOperationStartResult.Failed(),
        };
    }

    public bool Complete(FireBurningOperationExecution execution)
    {
        if (execution == null || execution.ResultItem == null || _inventory == null)
        {
            return false;
        }

        return _inventory.TryAddItem(execution.ResultItem, execution.ResultCount, execution.ResultAmountOverride, execution.ResultDurabilityOverride);
    }

    private FireBurningOperationStartResult AddFuel(FireBurningOperationPlan plan, FireSourceInteractable source)
    {
        if (plan.SourceItem == null || plan.SourceItem.BurnMinutes <= 0f)
        {
            return FireBurningOperationStartResult.Failed();
        }

        if (!_inventory.TryRemoveFromSlot(plan.SlotIndex, 1))
        {
            return FireBurningOperationStartResult.Failed();
        }

        source.AddFuel(plan.SourceItem.BurnMinutes);

        return FireBurningOperationStartResult.Completed();
    }

    private FireBurningOperationStartResult StartCooking(FireBurningOperationPlan plan, FireSourceInteractable source)
    {
        if (plan.SourceItem == null || !plan.SourceItem.CanBeCooked || plan.ResultItem == null)
        {
            return FireBurningOperationStartResult.Failed();
        }

        if (!source.HasEnoughBurnTime(plan.GameMinutes) || !_inventory.CanAddItem(plan.ResultItem, 1))
        {
            return FireBurningOperationStartResult.Failed();
        }

        if (!_inventory.TryRemoveFromSlot(plan.SlotIndex, 1))
        {
            return FireBurningOperationStartResult.Failed();
        }

        FireBurningOperationExecution execution = new(plan.Type, plan.GameMinutes, plan.ResultItem, resultDurabilityOverride: plan.ResultDurabilityOverride);

        return FireBurningOperationStartResult.Started(execution);
    }

    private FireBurningOperationStartResult StartMeltingSnow(FireBurningOperationPlan plan, FireSourceInteractable source)
    {
        if (plan.ResultItem == null || !plan.ResultItem.UsesCustomAmount || plan.Amount <= 0f)
        {
            return FireBurningOperationStartResult.Failed();
        }

        if (!source.HasEnoughBurnTime(plan.GameMinutes) || !_inventory.CanAddItem(plan.ResultItem, 1, plan.Amount))
        {
            return FireBurningOperationStartResult.Failed();
        }

        FireBurningOperationExecution execution = new(plan.Type, plan.GameMinutes, plan.ResultItem, resultAmountOverride: plan.Amount);

        return FireBurningOperationStartResult.Started(execution);
    }

    private FireBurningOperationStartResult StartBoilingWater(FireBurningOperationPlan plan, FireSourceInteractable source)
    {
        if (plan.SourceItem == null || plan.ResultItem == null || !plan.SourceItem.UsesCustomAmount || !plan.ResultItem.UsesCustomAmount || plan.Amount <= 0f)
        {
            return FireBurningOperationStartResult.Failed();
        }

        if (!source.HasEnoughBurnTime(plan.GameMinutes) || !_inventory.HasCustomAmount(plan.SourceItem, plan.Amount) || !_inventory.CanAddItem(plan.ResultItem, 1, plan.Amount))
        {
            return FireBurningOperationStartResult.Failed();
        }

        if (!_inventory.TryConsumeCustomAmountAcrossSlots(plan.SourceItem, plan.Amount))
        {
            return FireBurningOperationStartResult.Failed();
        }

        FireBurningOperationExecution execution = new(plan.Type, plan.GameMinutes, plan.ResultItem, resultAmountOverride: plan.Amount);

        return FireBurningOperationStartResult.Started(execution);
    }
}
