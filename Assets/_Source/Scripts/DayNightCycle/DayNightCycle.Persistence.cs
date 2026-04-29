using UnityEngine;

public partial class DayNightCycle
{
    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.DayNight.HasData = true;
        saveData.DayNight.Day = CurrentDay;
        saveData.DayNight.TimeOfDayMinutes = _timeOfDayMinutes;
        saveData.DayNight.IsRunning = _isRunning;
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.DayNight == null || !saveData.DayNight.HasData)
        {
            return;
        }

        CurrentDay = DayNightTimeMath.ClampDay(saveData.DayNight.Day);
        _timeOfDayMinutes = DayNightTimeMath.RepeatTimeOfDayMinutes(saveData.DayNight.TimeOfDayMinutes);
        _isRunning = saveData.DayNight.IsRunning;
        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();

        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }
}