using System;

[Serializable]
public sealed class FireSourceSaveData
{
    public string SaveId;

    public bool IsBurning;
    public float RemainingBurnGameMinutes;
}