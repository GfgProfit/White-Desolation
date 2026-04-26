using System;

[Serializable]
public sealed class InventorySlotSaveData
{
    public string ItemId;

    public int Count;

    public float CurrentDurability;
    public float CurrentAmount;
    public float CurrentHydration;
    public float CurrentCalories;
}