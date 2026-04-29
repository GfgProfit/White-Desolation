public static class PlayerTemperatureStateResolver
{
    public static PlayerTemperatureState Resolve(float temperature)
    {
        if (temperature <= 0f)
        {
            return PlayerTemperatureState.FatalHypothermia;
        }

        if (temperature <= 25f)
        {
            return PlayerTemperatureState.Hypothermia;
        }

        if (temperature <= 50f)
        {
            return PlayerTemperatureState.Cold;
        }

        if (temperature <= 75f)
        {
            return PlayerTemperatureState.Cool;
        }

        return PlayerTemperatureState.Warm;
    }

    public static bool IsFatalHypothermia(float temperature)
    {
        return temperature <= 0f;
    }

    public static bool IsHypothermia(float temperature)
    {
        return temperature > 0f && temperature <= 25f;
    }

    public static bool IsCold(float temperature)
    {
        return temperature > 25f && temperature <= 50f;
    }

    public static bool IsCool(float temperature)
    {
        return temperature > 50f && temperature <= 75f;
    }

    public static bool IsWarm(float temperature)
    {
        return temperature > 75f;
    }
}