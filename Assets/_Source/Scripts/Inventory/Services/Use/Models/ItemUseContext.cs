public readonly struct ItemUseContext
{
    public InventoryController Inventory { get; }
    public IPlayerNeeds PlayerNeeds { get; }
    public float UseDurationSeconds { get; }
    public bool IsUsingItem { get; }

    public ItemUseContext(InventoryController inventory, IPlayerNeeds playerNeeds, float useDurationSeconds, bool isUsingItem)
    {
        Inventory = inventory;
        PlayerNeeds = playerNeeds;
        UseDurationSeconds = useDurationSeconds;
        IsUsingItem = isUsingItem;
    }

    public bool HasInventory => Inventory != null;
    public bool HasPlayerNeeds => PlayerNeeds != null;
}
