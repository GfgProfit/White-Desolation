[System.Serializable]
public class InventorySlot
{
    public ItemData Item;
    public int Count;

    public int MaxStack => Item != null ? Item.MaxStack : 1;
    public bool IsEmpty => Item == null || Count <= 0;
    public bool IsFull => !IsEmpty && Count >= MaxStack;
}