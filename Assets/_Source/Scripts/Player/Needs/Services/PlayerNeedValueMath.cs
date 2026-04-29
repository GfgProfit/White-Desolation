using UnityEngine;

public static class PlayerNeedValueMath
{
    public static float Clamp(float value, float max)
    {
        return Mathf.Clamp(value, 0f, max);
    }

    public static float Normalize(float value, float max)
    {
        return Mathf.Clamp01(value / max);
    }

    public static float Missing(float value, float max)
    {
        return Mathf.Max(0f, max - value);
    }

    public static float GetRestoreAmount(float value, float max, float availableAmount)
    {
        if (availableAmount <= 0f)
        {
            return 0f;
        }

        float restored = Mathf.Min(Missing(value, max), availableAmount);
        return restored <= 0f ? 0f : restored;
    }
}