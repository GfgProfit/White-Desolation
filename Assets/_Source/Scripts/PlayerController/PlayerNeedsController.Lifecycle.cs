using UnityEngine;

public partial class PlayerNeedsController
{
    private void Awake()
    {
        _presenter = new PlayerNeedsPresenter(_temperatureFill, _fatigueFill, _thirstFill, _hungerFill);

        _temperature = PlayerNeedValueMath.Clamp(_startTemperature, _maxTemperature);
        _fatigue = PlayerNeedValueMath.Clamp(_startFatigue, _maxFatigue);
        _thirst = PlayerNeedValueMath.Clamp(_startThirst, _maxThirst);
        _hunger = PlayerNeedValueMath.Clamp(_startHunger, _maxHunger);

        RefreshUI();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        TickTemperature(deltaTime);
        TickFatigue(deltaTime);
        TickThirst(deltaTime);
        TickHunger(deltaTime);

        RefreshUI();
    }
}