using System;
using System.Collections.Generic;

[Serializable]
public sealed class GameSaveData
{
    public int Version = 1;

    public PlayerTransformSaveData PlayerTransform = new();
    public PlayerNeedsSaveData PlayerNeeds = new();
    public DayNightSaveData DayNight = new();

    public List<InventorySlotSaveData> InventorySlots = new();
    public List<CrateSaveData> Crates = new();
    public List<WorldItemSaveData> WorldItems = new();
    public List<FireSourceSaveData> FireSources = new();
}
