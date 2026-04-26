using System;

[Serializable]
public sealed class WorldItemSaveData
{
    public string SaveId;

    public bool PickedUp;

    public string ItemId;
    public int Count;

    public bool OverrideCurrentAmount;
    public float CurrentAmount;

    public bool OverrideCurrentDurability;
    public float CurrentDurability;

    public SerializableVector3 Position;
    public SerializableQuaternion Rotation;
}