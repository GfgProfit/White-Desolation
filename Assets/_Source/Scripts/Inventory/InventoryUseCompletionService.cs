public sealed class InventoryUseCompletionService
{
    private readonly InventoryController _inventoryController;

    public InventoryUseCompletionService(InventoryController inventoryController)
    {
        _inventoryController = inventoryController;
    }

    public InventoryUseCompletionResult Complete(ItemUsePlan plan, ItemUseContext nextUseContext)
    {
        if (_inventoryController == null || plan == null)
        {
            return InventoryUseCompletionResult.Failed();
        }

        if (!TryConsumeToolDurability(plan))
        {
            return InventoryUseCompletionResult.Failed();
        }

        ApplySlotReplacement(plan);
        ApplyInventoryConsume(plan);

        if (TryBuildAutoUsePlan(plan, nextUseContext, out ItemUsePlan nextPlan))
        {
            return InventoryUseCompletionResult.ContinueWith(nextPlan);
        }

        return InventoryUseCompletionResult.Completed();
    }

    private bool TryConsumeToolDurability(ItemUsePlan plan)
    {
        if (!plan.HasToolDurabilityConsume)
        {
            return true;
        }

        return _inventoryController.TryConsumeDurabilityFromFirstMatchingItem(plan.ToolItemToDamage, plan.ToolDurabilityCost);
    }

    private void ApplySlotReplacement(ItemUsePlan plan)
    {
        if (plan.ReplaceSlotItemAfterAction == null)
        {
            return;
        }

        _inventoryController.TryReplaceSlotItem(plan.SlotIndex, plan.ReplaceSlotItemAfterAction);
    }

    private void ApplyInventoryConsume(ItemUsePlan plan)
    {
        if (!plan.HasInventoryConsume)
        {
            return;
        }

        _inventoryController.TryConsumeFromSlot(plan.SlotIndex, plan.HydrationStateToConsume, plan.CaloriesStateToConsume, plan.AmountToConsume, plan.ReplaceWhenDepleted);
    }

    private bool TryBuildAutoUsePlan(ItemUsePlan completedPlan, ItemUseContext nextUseContext, out ItemUsePlan nextPlan)
    {
        nextPlan = null;

        if (!completedPlan.AutoUseReplacedItem)
        {
            return false;
        }

        InventorySlot nextSlot = _inventoryController.GetSlotAt(completedPlan.SlotIndex);

        if (nextSlot == null || nextSlot.Item == null)
        {
            return false;
        }

        return ItemUseService.TryBuildPlan(nextUseContext, completedPlan.SlotIndex, nextSlot, out nextPlan);
    }
}