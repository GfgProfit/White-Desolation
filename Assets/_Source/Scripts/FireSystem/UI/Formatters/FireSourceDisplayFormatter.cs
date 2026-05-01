using UnityEngine;

public static class FireSourceDisplayFormatter
{
    public static string FormatBurnTime(float gameMinutes)
    {
        int totalMinutes = Mathf.CeilToInt(Mathf.Max(0f, gameMinutes));
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        if (hours > 0)
        {
            return $"{hours} ч {minutes:00} мин";
        }

        return $"{minutes} мин";
    }

    public static string FormatTemperature(float temperatureCelsius)
    {
        return $"{temperatureCelsius:0.#} °C";
    }
}
