public partial class PlayerNeedsController
{
    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.PlayerNeeds.HasData = true;
        saveData.PlayerNeeds.Temperature = _temperature;
        saveData.PlayerNeeds.Fatigue = _fatigue;
        saveData.PlayerNeeds.Thirst = _thirst;
        saveData.PlayerNeeds.Hunger = _hunger;
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.PlayerNeeds == null || !saveData.PlayerNeeds.HasData)
        {
            return;
        }

        _temperature = PlayerNeedValueMath.Clamp(saveData.PlayerNeeds.Temperature, _maxTemperature);
        _fatigue = PlayerNeedValueMath.Clamp(saveData.PlayerNeeds.Fatigue, _maxFatigue);
        _thirst = PlayerNeedValueMath.Clamp(saveData.PlayerNeeds.Thirst, _maxThirst);
        _hunger = PlayerNeedValueMath.Clamp(saveData.PlayerNeeds.Hunger, _maxHunger);

        RefreshUI();
    }
}