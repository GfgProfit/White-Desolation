using System.Collections.Generic;

public partial class InventoryController
{
    public bool CanAddItem(ItemData itemData, int count, float? currentAmountOverride = null) => InventoryAddCapacityPolicy.CanAddItem(CurrentCarryWeightKg, _maxCarryWeightKg, itemData, count, currentAmountOverride);
    public InventorySlot GetSlotAt(int slotIndex) => InventorySlotQuery.GetSlotOrNull(_items, slotIndex);
    public bool ContainsUsableItem(ItemData itemData, int count = 1) => InventoryItemQuery.ContainsUsableItem(_items, itemData, count);
    public int GetTotalCount(ItemData itemData) => InventoryItemQuery.GetTotalCount(_items, itemData);
    public float GetTotalAmount(ItemData itemData) => InventoryItemQuery.GetTotalAmount(_items, itemData);
    public float GetCurrentTotalWeightKg() => InventoryWeightCalculator.CalculateTotalWeightKg(_items);
    public bool Contains(ItemData itemData, int count = 1) => InventoryItemQuery.Contains(_items, itemData, count);
    public bool HasCustomAmount(ItemData item, float requiredAmount) => InventoryItemQuery.HasCustomAmount(_items, item, requiredAmount);
    public bool Contains(List<ItemData> items, ItemData item) => ItemDataListQuery.Contains(items, item);
}