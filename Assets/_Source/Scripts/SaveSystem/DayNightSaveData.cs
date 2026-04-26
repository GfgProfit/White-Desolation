using System;

[Serializable]
public sealed class DayNightSaveData
{
    public bool HasData;

    public int Day;
    public float TimeOfDayMinutes;
    public bool IsRunning;
}