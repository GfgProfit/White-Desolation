using System;

[Serializable]
public sealed class PlayerNeedsSaveData
{
    public bool HasData;

    public float Temperature;
    public float Fatigue;
    public float Thirst;
    public float Hunger;
}