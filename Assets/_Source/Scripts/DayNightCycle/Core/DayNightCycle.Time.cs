using UnityEngine;

public partial class DayNightCycle
{
    public void StartTime()
    {
        _isRunning = true;
    }

    public void StopTime()
    {
        _isRunning = false;
    }

    public void SetTime(int hour, int minute)
    {
        hour = DayNightTimeMath.ClampHour(hour);
        minute = DayNightTimeMath.ClampMinute(minute);

        _timeOfDayMinutes = DayNightTimeMath.ToTimeOfDayMinutes(hour, minute);
        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void SetDay(int day)
    {
        CurrentDay = DayNightTimeMath.ClampDay(day);
        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void SetDateTime(int day, int hour, int minute)
    {
        CurrentDay = DayNightTimeMath.ClampDay(day);
        hour = DayNightTimeMath.ClampHour(hour);
        minute = DayNightTimeMath.ClampMinute(minute);

        _timeOfDayMinutes = DayNightTimeMath.ToTimeOfDayMinutes(hour, minute);
        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();
        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public string GetFormattedTime()
    {
        return DayNightTimeMath.FormatTime(_timeOfDayMinutes);
    }

    public float RealSecondsToGameMinutes(float realSeconds)
    {
        return DayNightTimeMath.RealSecondsToGameMinutes(realSeconds, _realSecondsPerGameDay);
    }

    public float GameMinutesToRealSeconds(float gameMinutes)
    {
        return DayNightTimeMath.GameMinutesToRealSeconds(gameMinutes, _realSecondsPerGameDay);
    }

    public void SetTimeOfDayMinutes(float timeOfDayMinutes)
    {
        _timeOfDayMinutes = DayNightTimeMath.RepeatTimeOfDayMinutes(timeOfDayMinutes);
        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void SetRunning(bool running)
    {
        _isRunning = running;
    }

    private void AdvanceTime(float deltaTime)
    {
        float gameMinutesPerSecond = DayNightTimeMath.GetGameMinutesPerRealSecond(_realSecondsPerGameDay);

        if (gameMinutesPerSecond <= 0f)
        {
            return;
        }

        AdvanceGameMinutes(gameMinutesPerSecond * deltaTime);
    }

    private void AdvanceGameMinutes(float gameMinutes)
    {
        if (Mathf.Approximately(gameMinutes, 0f))
        {
            return;
        }

        _timeOfDayMinutes += gameMinutes;

        while (_timeOfDayMinutes >= DayNightTimeMath.MinutesPerDay)
        {
            _timeOfDayMinutes -= DayNightTimeMath.MinutesPerDay;
            CurrentDay++;
            OnDayChanged?.Invoke(CurrentDay);
        }

        while (_timeOfDayMinutes < 0f)
        {
            _timeOfDayMinutes += DayNightTimeMath.MinutesPerDay;
            CurrentDay = DayNightTimeMath.ClampDay(CurrentDay - 1);
            OnDayChanged?.Invoke(CurrentDay);
        }

        OnGameMinutesAdvanced?.Invoke(gameMinutes);
    }

    private void ReportMinuteChangeIfNeeded()
    {
        int wholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        if (wholeMinute == _lastReportedWholeMinute)
        {
            return;
        }

        _lastReportedWholeMinute = wholeMinute;
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void AddGameMinutes(float gameMinutes)
    {
        if (Mathf.Approximately(gameMinutes, 0f))
        {
            return;
        }

        AdvanceGameMinutes(gameMinutes);

        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }
}
