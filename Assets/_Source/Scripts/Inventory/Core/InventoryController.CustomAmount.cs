public partial class InventoryController
{
    public bool TryConsumeCustomAmountAcrossSlots(ItemData itemData, float amount)
    {
        bool consumed = InventoryCustomAmountConsumeService.TryConsumeAcrossSlots(_items, itemData, amount, ZeroTolerance);

        return FinishInventoryMutation(consumed);
    }
}
