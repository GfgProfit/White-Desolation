using UnityEngine;

public static class InventoryConsumableInstancePolicy
{
    public static bool RequiresDedicatedInstance(ItemData itemData)
    {
        return itemData != null && (!Mathf.Approximately(itemData.RestoreHydration, 0f) || itemData.RestoreCalories != 0);
    }
}