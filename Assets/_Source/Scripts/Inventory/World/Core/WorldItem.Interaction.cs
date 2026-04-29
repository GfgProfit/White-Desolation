public partial class WorldItem
{
    public InteractionHoverInfo GetHoverInfo() => WorldItemInteractionInfoBuilder.BuildHoverInfo(_itemData, CurrentDurability);
    public InteractionInspectInfo GetInspectInfo() => WorldItemInteractionInfoBuilder.BuildInspectInfo(_itemData, CurrentDurability, CurrentWeightKg);
    public bool TryConfirmInspectAction() => TryPickup();
}