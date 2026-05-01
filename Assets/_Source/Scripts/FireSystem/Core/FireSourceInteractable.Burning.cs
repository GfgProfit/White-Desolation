using UnityEngine;

public sealed partial class FireSourceInteractable
{
    public void Ignite(float burnGameMinutes)
    {
        _remainingBurnGameMinutes = Mathf.Max(0f, burnGameMinutes);
        _isBurning = _remainingBurnGameMinutes > 0f;
    }

    public void Extinguish()
    {
        _isBurning = false;
        _remainingBurnGameMinutes = 0f;
    }

    public void AddFuel(float burnGameMinutes)
    {
        if (burnGameMinutes <= 0f)
        {
            return;
        }

        _remainingBurnGameMinutes = Mathf.Max(0f, _remainingBurnGameMinutes) + burnGameMinutes;
        _isBurning = _remainingBurnGameMinutes > 0f;
    }

    public bool HasEnoughBurnTime(float gameMinutes)
    {
        return _isBurning && _remainingBurnGameMinutes >= Mathf.Max(0f, gameMinutes);
    }

    public void ConsumeBurnTime(float gameMinutes)
    {
        if (!_isBurning || gameMinutes <= 0f)
        {
            return;
        }

        _remainingBurnGameMinutes = Mathf.Max(0f, _remainingBurnGameMinutes - gameMinutes);

        if (_remainingBurnGameMinutes <= 0f)
        {
            Extinguish();
        }
    }

    private float GameMinutesToRealSeconds(float gameMinutes)
    {
        if (_gameTimeConverter == null)
        {
            return gameMinutes * 60f;
        }

        return _gameTimeConverter.GameMinutesToRealSeconds(gameMinutes);
    }
}
