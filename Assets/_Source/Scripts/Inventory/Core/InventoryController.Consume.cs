using UnityEngine;

public partial class InventoryController
{
    public bool TryConsumeFromSlot(int slotIndex, float hydrationToConsume = 0f, float caloriesToConsume = 0f, float amountToConsume = 0f, ItemData replaceWhenDepleted = null)
    {
        if (!InventorySlotQuery.TryGetNonEmptySlotWithItem(_items, slotIndex, out InventorySlot slot))
        {
            return false;
        }

        InventorySlotConsumeApplier.ApplyConsume(slot, hydrationToConsume, caloriesToConsume, amountToConsume, ZeroTolerance);

        InventoryConsumedSlotMutationService.RemoveOrReplaceIfDepleted(_items, slotIndex, slot, replaceWhenDepleted);

        NotifyChanged();
        return true;
    }

    public bool TryConsumeDurabilityFromFirstMatchingItem(ItemData itemData, float durabilityCost)
    {
        InventoryDurabilityConsumeResult result = InventoryDurabilityConsumeService.TryConsumeFromFirstMatchingItem(_items, itemData, durabilityCost);

        if (result == InventoryDurabilityConsumeResult.Failed)
        {
            return false;
        }

        if (result == InventoryDurabilityConsumeResult.Mutated)
        {
            NotifyChanged();
        }

        return true;
    }

    public bool TryConsumeDurabilityFromSlot(int slotIndex, ItemData expectedItem, float durabilityCost)
    {
        InventoryDurabilityConsumeResult result = InventoryDurabilityConsumeService.TryConsumeFromSlot(_items, slotIndex, expectedItem, durabilityCost);

        if (result == InventoryDurabilityConsumeResult.Failed)
        {
            return false;
        }

        if (result == InventoryDurabilityConsumeResult.Mutated)
        {
            NotifyChanged();
        }

        return true;
    }

    public bool TryConsumeCustomAmountFromFirstMatchingItem(ItemData itemData, float amount)
    {
        if (itemData == null)
        {
            UnityEngine.Debug.LogWarning("Cannot consume amount: ItemData is null.");
            return false;
        }

        if (amount <= 0f)
        {
            Debug.LogWarning($"Cannot consume amount from {itemData.DisplayName}: amount must be > 0.");
            return false;
        }

        if (!InventoryCustomAmountSlotQuery.TryFindFirstMatchingSlotIndex(_items, itemData, amount, ZeroTolerance, out int slotIndex))
        {
            Debug.LogWarning($"Not enough amount in {itemData.DisplayName}. Need {amount}.");
            return false;
        }

        return TryConsumeFromSlot(slotIndex, amountToConsume: amount);
    }

    public bool TryConsumeCustomAmount(ItemData item, float amount)
    {
        if (!InventoryItemQuery.TryFindCustomAmountSlotIndex(_items, item, amount, out int slotIndex))
        {
            return false;
        }

        return TryConsumeFromSlot(slotIndex, amountToConsume: amount);
    }
}
