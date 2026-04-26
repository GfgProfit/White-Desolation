public readonly struct ItemUseContext
{
    public InventoryController Inventory { get; }
    public PlayerNeedsController PlayerNeeds { get; }
    public float UseDurationSeconds { get; }
    public bool IsUsingItem { get; }

    public ItemUseContext(InventoryController inventory, PlayerNeedsController playerNeeds, float useDurationSeconds, bool isUsingItem)
    {
        Inventory = inventory;
        PlayerNeeds = playerNeeds;
        UseDurationSeconds = useDurationSeconds;
        IsUsingItem = isUsingItem;
    }

    public bool HasInventory => Inventory != null;
    public bool HasPlayerNeeds => PlayerNeeds != null;
}