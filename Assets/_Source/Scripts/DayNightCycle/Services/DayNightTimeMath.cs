using UnityEngine;

public static class DayNightTimeMath
{
    public const float MinutesPerHour = 60f;
    public const float MinutesPerDay = 1440f;

    public static int ClampDay(int day)
    {
        return Mathf.Max(1, day);
    }

    public static int ClampHour(int hour)
    {
        return Mathf.Clamp(hour, 0, 23);
    }

    public static int ClampMinute(int minute)
    {
        return Mathf.Clamp(minute, 0, 59);
    }

    public static float ToTimeOfDayMinutes(int hour, int minute)
    {
        return ClampHour(hour) * MinutesPerHour + ClampMinute(minute);
    }

    public static float RepeatTimeOfDayMinutes(float timeOfDayMinutes)
    {
        return Mathf.Repeat(timeOfDayMinutes, MinutesPerDay);
    }

    public static int GetHour(float timeOfDayMinutes)
    {
        return Mathf.FloorToInt(timeOfDayMinutes / MinutesPerHour) % 24;
    }

    public static int GetMinute(float timeOfDayMinutes)
    {
        return Mathf.FloorToInt(timeOfDayMinutes % MinutesPerHour);
    }

    public static int GetWholeMinute(float timeOfDayMinutes)
    {
        return Mathf.FloorToInt(timeOfDayMinutes);
    }

    public static float GetNormalizedTimeOfDay(float timeOfDayMinutes)
    {
        return timeOfDayMinutes / MinutesPerDay;
    }

    public static string FormatTime(float timeOfDayMinutes)
    {
        return $"{GetHour(timeOfDayMinutes):00}:{GetMinute(timeOfDayMinutes):00}";
    }

    public static float RealSecondsToGameMinutes(float realSeconds, float realSecondsPerGameDay)
    {
        if (realSecondsPerGameDay <= 0.01f)
        {
            return 0f;
        }

        return Mathf.Max(0f, realSeconds) * MinutesPerDay / realSecondsPerGameDay;
    }

    public static float GameMinutesToRealSeconds(float gameMinutes, float realSecondsPerGameDay)
    {
        if (realSecondsPerGameDay <= 0.01f)
        {
            return 0f;
        }

        return Mathf.Max(0f, gameMinutes) * realSecondsPerGameDay / MinutesPerDay;
    }

    public static float GetGameMinutesPerRealSecond(float realSecondsPerGameDay)
    {
        if (realSecondsPerGameDay <= 0.01f)
        {
            return 0f;
        }

        return MinutesPerDay / realSecondsPerGameDay;
    }
}