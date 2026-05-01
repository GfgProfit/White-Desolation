public readonly struct CraftToolCandidate
{
    public int SlotIndex { get; }
    public InventorySlot Slot { get; }
    public CraftToolRequirement Requirement { get; }

    public ItemData Tool => Slot?.Item;
    public float DurabilityCost => Requirement != null ? Requirement.DurabilityCost : 0f;

    public CraftToolCandidate(int slotIndex, InventorySlot slot, CraftToolRequirement requirement)
    {
        SlotIndex = slotIndex;
        Slot = slot;
        Requirement = requirement;
    }
}
