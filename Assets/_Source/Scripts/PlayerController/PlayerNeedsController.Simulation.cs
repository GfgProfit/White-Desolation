using UnityEngine;

public partial class PlayerNeedsController
{
    private void TickTemperature(float dt)
    {
        if (Mathf.Approximately(_temperatureDeltaPerSecond, 0f))
        {
            return;
        }

        _temperature = PlayerNeedValueMath.Clamp(_temperature + (_temperatureDeltaPerSecond * dt), _maxTemperature);
    }

    private void TickFatigue(float dt)
    {
        float multiplier = PlayerNeedsLocomotionMultiplierResolver.Resolve(_locomotionState, _fatigueIdleMultiplier, _fatigueWalkMultiplier, _fatigueRunMultiplier);
        _fatigue = PlayerNeedValueMath.Clamp(_fatigue - (_fatigueDrainPerSecond * multiplier * dt), _maxFatigue);
    }

    private void TickThirst(float dt)
    {
        float multiplier = PlayerNeedsLocomotionMultiplierResolver.Resolve(_locomotionState, _thirstIdleMultiplier, _thirstWalkMultiplier, _thirstRunMultiplier);
        _thirst = PlayerNeedValueMath.Clamp(_thirst - (_thirstDrainPerSecond * multiplier * dt), _maxThirst);
    }

    private void TickHunger(float dt)
    {
        float multiplier = PlayerNeedsLocomotionMultiplierResolver.Resolve(_locomotionState, _hungerIdleMultiplier, _hungerWalkMultiplier, _hungerRunMultiplier);
        _hunger = PlayerNeedValueMath.Clamp(_hunger - (_hungerDrainPerSecond * multiplier * dt), _maxHunger);
    }
}