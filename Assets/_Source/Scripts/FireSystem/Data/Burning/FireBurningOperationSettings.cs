using UnityEngine;

public readonly struct FireBurningOperationSettings
{
    public float WaterStepLiters { get; }
    public float MeltSnowMaxLiters { get; }
    public float MeltSnowGameMinutesPerStep { get; }
    public float BoilWaterGameMinutesPerStep { get; }

    public FireBurningOperationSettings(float waterStepLiters, float meltSnowMaxLiters, float meltSnowGameMinutesPerStep, float boilWaterGameMinutesPerStep)
    {
        WaterStepLiters = Mathf.Max(0.01f, waterStepLiters);
        MeltSnowMaxLiters = Mathf.Max(0f, meltSnowMaxLiters);
        MeltSnowGameMinutesPerStep = Mathf.Max(0f, meltSnowGameMinutesPerStep);
        BoilWaterGameMinutesPerStep = Mathf.Max(0f, boilWaterGameMinutesPerStep);
    }

    public float GetMinWaterAmount(float maxAmount)
    {
        if (maxAmount <= 0f)
        {
            return 0f;
        }

        return Mathf.Min(WaterStepLiters, maxAmount);
    }

    public float GetGameMinutes(FireBurningWaterMode mode, float amount)
    {
        float minutesPerStep = mode == FireBurningWaterMode.MeltSnow ? MeltSnowGameMinutesPerStep : BoilWaterGameMinutesPerStep;

        return amount / WaterStepLiters * minutesPerStep;
    }

    public float RoundAmount(float amount)
    {
        return Mathf.Round(amount * 100f) / 100f;
    }
}
