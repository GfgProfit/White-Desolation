public readonly struct InventoryViewEntry
{
    public int SlotIndex { get; }
    public InventorySlot Slot { get; }

    public InventoryViewEntry(int slotIndex, InventorySlot slot)
    {
        SlotIndex = slotIndex;
        Slot = slot;
    }
}