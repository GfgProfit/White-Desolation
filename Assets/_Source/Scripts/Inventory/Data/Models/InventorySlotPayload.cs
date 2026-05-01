public readonly struct InventorySlotPayload
{
    public InventorySlotPayload(
        ItemData item,
        int count,
        float? currentAmount,
        float? currentDurability,
        float? currentHydration,
        float? currentCalories)
    {
        Item = item;
        Count = count;
        CurrentAmount = currentAmount;
        CurrentDurability = currentDurability;
        CurrentHydration = currentHydration;
        CurrentCalories = currentCalories;
    }

    public ItemData Item { get; }
    public int Count { get; }
    public float? CurrentAmount { get; }
    public float? CurrentDurability { get; }
    public float? CurrentHydration { get; }
    public float? CurrentCalories { get; }

    public bool IsValid => Item != null && Count > 0;

    public static bool TryCreate(InventorySlot slot, int requestedCount, out InventorySlotPayload payload)
    {
        payload = default;

        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return false;
        }

        int count = requestedCount < 1 ? 1 : requestedCount;

        if (count > slot.Count)
        {
            count = slot.Count;
        }

        payload = new InventorySlotPayload(
            slot.Item,
            count,
            slot.HasAmount ? slot.CurrentAmount : null,
            slot.HasDurability ? slot.CurrentDurability : null,
            slot.HasConsumableState ? slot.CurrentHydration : null,
            slot.HasConsumableState ? slot.CurrentCalories : null);

        return payload.IsValid;
    }
}
