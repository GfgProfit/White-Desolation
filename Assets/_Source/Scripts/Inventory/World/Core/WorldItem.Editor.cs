using NaughtyAttributes;

public partial class WorldItem
{
    [Button]
    private void AssignObjectName()
    {
        gameObject.name = $"[Item] - ID: {_itemData.Id}";
    }
}