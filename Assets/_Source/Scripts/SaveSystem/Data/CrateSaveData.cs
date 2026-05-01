using System;
using System.Collections.Generic;

[Serializable]
public sealed class CrateSaveData
{
    public string SaveId;
    public bool LootGenerated;
    public bool Searched;
    public List<InventorySlotSaveData> Items = new();
}
