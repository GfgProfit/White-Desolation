using UnityEngine.UI;

public sealed class PlayerNeedsPresenter
{
    private readonly Image _temperatureFill;
    private readonly Image _fatigueFill;
    private readonly Image _thirstFill;
    private readonly Image _hungerFill;

    public PlayerNeedsPresenter(Image temperatureFill, Image fatigueFill, Image thirstFill, Image hungerFill)
    {
        _temperatureFill = temperatureFill;
        _fatigueFill = fatigueFill;
        _thirstFill = thirstFill;
        _hungerFill = hungerFill;
    }

    public void Refresh(float temperatureNormalized, float fatigueNormalized, float thirstNormalized, float hungerNormalized)
    {
        if (_temperatureFill != null)
        {
            _temperatureFill.fillAmount = temperatureNormalized;
        }

        if (_fatigueFill != null)
        {
            _fatigueFill.fillAmount = fatigueNormalized;
        }

        if (_thirstFill != null)
        {
            _thirstFill.fillAmount = thirstNormalized;
        }

        if (_hungerFill != null)
        {
            _hungerFill.fillAmount = hungerNormalized;
        }
    }
}