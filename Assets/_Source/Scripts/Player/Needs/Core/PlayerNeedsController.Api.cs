public partial class PlayerNeedsController
{
    public void SetLocomotionState(PlayerLocomotionState locomotionState)
    {
        _locomotionState = locomotionState;
    }

    public void SetTemperatureDeltaPerSecond(float value)
    {
        _temperatureDeltaPerSecond = value;
    }

    public void AddTemperature(float delta)
    {
        _temperature = PlayerNeedValueMath.Clamp(_temperature + delta, _maxTemperature);
        RefreshUI();
    }

    public void AddFatigue(float delta)
    {
        _fatigue = PlayerNeedValueMath.Clamp(_fatigue + delta, _maxFatigue);
        RefreshUI();
    }

    public void AddThirst(float delta)
    {
        _thirst = PlayerNeedValueMath.Clamp(_thirst + delta, _maxThirst);
        RefreshUI();
    }

    public void AddHunger(float delta)
    {
        _hunger = PlayerNeedValueMath.Clamp(_hunger + delta, _maxHunger);
        RefreshUI();
    }

    public float RestoreThirstUpTo(float availableHydration)
    {
        float restored = PlayerNeedValueMath.GetRestoreAmount(_thirst, _maxThirst, availableHydration);

        if (restored <= 0f)
        {
            return 0f;
        }

        AddThirst(restored);

        return restored;
    }

    public float RestoreHungerUpTo(float availableCalories)
    {
        float restored = PlayerNeedValueMath.GetRestoreAmount(_hunger, _maxHunger, availableCalories);

        if (restored <= 0f)
        {
            return 0f;
        }

        AddHunger(restored);

        return restored;
    }

    public void ApplyConsumable(float hydrationValue, float caloriesValue)
    {
        AddThirst(hydrationValue);
        AddHunger(caloriesValue);
    }
}