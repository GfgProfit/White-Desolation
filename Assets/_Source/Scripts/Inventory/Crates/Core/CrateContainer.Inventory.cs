public sealed partial class CrateContainer
{
    public bool TryAddFromSlot(InventorySlot sourceSlot, int count = 1)
    {
        if (!InventorySlotPayload.TryCreate(sourceSlot, count, out InventorySlotPayload payload))
        {
            return false;
        }

        return TryAddItem(
            payload.Item,
            payload.Count,
            payload.CurrentAmount,
            payload.CurrentDurability,
            payload.CurrentHydration,
            payload.CurrentCalories);
    }

    public bool TryAddItem(ItemData item, int count, float? currentAmountOverride = null, float? currentDurabilityOverride = null, float? currentHydrationOverride = null, float? currentCaloriesOverride = null)
    {
        bool added = CrateInventoryAddService.TryAddItem(
            _items,
            _maxWeightKg,
            item,
            count,
            currentAmountOverride,
            currentDurabilityOverride,
            currentHydrationOverride,
            currentCaloriesOverride);

        return FinishMutation(added);
    }

    public bool TryRemoveFromSlot(int slotIndex, int count = 1)
    {
        bool removed = InventorySlotRemovalService.TryRemoveFromSlot(_items, slotIndex, count);

        return FinishMutation(removed);
    }

    public bool TryRemoveFromSlot(InventorySlot slot, int count = 1)
    {
        int slotIndex = IndexOf(slot);

        if (slotIndex < 0)
        {
            return false;
        }

        return TryRemoveFromSlot(slotIndex, count);
    }

    public InventorySlot GetSlotAt(int slotIndex)
    {
        return InventorySlotQuery.GetSlotOrNull(_items, slotIndex);
    }

    public int IndexOf(InventorySlot slot)
    {
        if (slot == null)
        {
            return -1;
        }

        for (int i = 0; i < _items.Count; i++)
        {
            if (ReferenceEquals(_items[i], slot))
            {
                return i;
            }
        }

        return -1;
    }
}
