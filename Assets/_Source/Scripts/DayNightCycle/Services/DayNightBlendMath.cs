using UnityEngine;

public static class DayNightBlendMath
{
    public static float EvaluateDayFactor(float normalizedTimeOfDay, float sunriseStart, float sunriseEnd, float sunsetStart, float sunsetEnd)
    {
        float sunrise = SmoothStep01(sunriseStart, sunriseEnd, normalizedTimeOfDay);
        float sunset = 1f - SmoothStep01(sunsetStart, sunsetEnd, normalizedTimeOfDay);
        return Mathf.Clamp01(sunrise * sunset);
    }

    public static float EvaluateNightFactor(float dayFactor)
    {
        return 1f - dayFactor;
    }

    public static float SmoothStep01(float start, float end, float value)
    {
        if (Mathf.Approximately(start, end))
        {
            return value >= end ? 1f : 0f;
        }

        float x = Mathf.InverseLerp(start, end, value);
        return x * x * (3f - 2f * x);
    }
}