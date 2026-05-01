public static class CrateTransferService
{
    public static bool TryMoveFromInventoryToCrate(InventoryController source, CrateContainer destination, int sourceSlotIndex, int count = 1)
    {
        if (source == null || destination == null)
        {
            return false;
        }

        InventorySlot sourceSlot = source.GetSlotAt(sourceSlotIndex);

        if (!InventorySlotPayload.TryCreate(sourceSlot, count, out InventorySlotPayload payload))
        {
            return false;
        }

        if (!source.TryRemoveFromSlot(sourceSlotIndex, payload.Count))
        {
            return false;
        }

        if (TryAddToCrate(destination, payload))
        {
            return true;
        }

        TryAddToInventory(source, payload);
        return false;
    }

    public static bool TryMoveFromCrateToInventory(CrateContainer source, InventoryController destination, int sourceSlotIndex, int count = 1)
    {
        if (source == null || destination == null)
        {
            return false;
        }

        InventorySlot sourceSlot = source.GetSlotAt(sourceSlotIndex);

        if (!InventorySlotPayload.TryCreate(sourceSlot, count, out InventorySlotPayload payload))
        {
            return false;
        }

        if (!source.TryRemoveFromSlot(sourceSlotIndex, payload.Count))
        {
            return false;
        }

        if (TryAddToInventory(destination, payload))
        {
            return true;
        }

        TryAddToCrate(source, payload);
        return false;
    }

    private static bool TryAddToInventory(InventoryController inventory, InventorySlotPayload payload)
    {
        return inventory != null
            && payload.IsValid
            && inventory.TryAddItem(
                payload.Item,
                payload.Count,
                payload.CurrentAmount,
                payload.CurrentDurability,
                payload.CurrentHydration,
                payload.CurrentCalories);
    }

    private static bool TryAddToCrate(CrateContainer crate, InventorySlotPayload payload)
    {
        return crate != null
            && payload.IsValid
            && crate.TryAddItem(
                payload.Item,
                payload.Count,
                payload.CurrentAmount,
                payload.CurrentDurability,
                payload.CurrentHydration,
                payload.CurrentCalories);
    }
}
