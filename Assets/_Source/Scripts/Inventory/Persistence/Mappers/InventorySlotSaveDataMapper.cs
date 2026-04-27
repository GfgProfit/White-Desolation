using UnityEngine;

public static class InventorySlotSaveDataMapper
{
    public static bool TryCreateSaveData(InventorySlot slot, out InventorySlotSaveData saveData)
    {
        saveData = null;

        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            return false;
        }

        saveData = new InventorySlotSaveData
        {
            ItemId = slot.Item.Id,
            Count = slot.Count,
            CurrentDurability = slot.CurrentDurability,
            CurrentAmount = slot.CurrentAmount,
            CurrentHydration = slot.CurrentHydration,
            CurrentCalories = slot.CurrentCalories
        };

        return true;
    }

    public static bool TryCreateSlot(InventorySlotSaveData slotData, SaveContext context, out InventorySlot slot)
    {
        slot = null;

        if (slotData == null)
        {
            return false;
        }

        if (context == null || context.ItemDatabase == null)
        {
            Debug.LogWarning("[Inventory] Cannot restore inventory: ItemDatabase is missing.");
            return false;
        }

        if (!context.ItemDatabase.TryGetItem(slotData.ItemId, out ItemData itemData))
        {
            Debug.LogWarning($"[Inventory] Cannot restore item. Unknown item id: {slotData.ItemId}");
            return false;
        }

        slot = InventorySlotFactory.Create(itemData, Mathf.Max(1, slotData.Count), slotData.CurrentDurability, slotData.CurrentAmount, slotData.CurrentHydration, slotData.CurrentCalories);

        return true;
    }
}