using UnityEngine;

public sealed class FireSourceInteractable : MonoBehaviour, IInteractable, IInteractHoverInfo
{
    [Header("Display")]
    [SerializeField] private string _displayName = "Печка";

    [Header("Runtime")]
    [SerializeField] private bool _isBurning;
    [SerializeField, Min(0f)] private float _remainingBurnGameMinutes;

    [Header("Temperature")]
    [SerializeField] private float _temperatureCelsius = 0f;

    [Inject] private readonly FireUIController _fireStartingUI;
    [Inject] private readonly DayNightCycle _dayNightCycle;

    public string DisplayName => _displayName;
    public bool IsBurning => _isBurning;
    public float RemainingBurnSeconds => GameMinutesToRealSeconds(_remainingBurnGameMinutes);
    public float RemainingBurnMinutes => _remainingBurnGameMinutes;
    public float TemperatureCelsius => _temperatureCelsius;

    private void Update()
    {
        if (!_isBurning)
        {
            return;
        }

        float gameMinutesDelta = RealSecondsToGameMinutes(Time.deltaTime);
        _remainingBurnGameMinutes = Mathf.Max(0f, _remainingBurnGameMinutes - gameMinutesDelta);

        if (_remainingBurnGameMinutes <= 0f)
        {
            Extinguish();
        }
    }

    public void Interact()
    {
        if (_isBurning)
        {
            return;
        }

        _fireStartingUI.OpenFireStarting(this);
    }

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

    public InteractionHoverInfo GetHoverInfo()
    {
        InteractionHoverInfo info = new InteractionHoverInfo
        {
            InteractionText = _displayName
        };

        if (!_isBurning)
        {
            return info;
        }

        info.TimeText = $"{FormatMinutes(_remainingBurnGameMinutes)}";
        info.TemperatureText = $"{_temperatureCelsius:0.#} °C";

        return info;
    }

    private float RealSecondsToGameMinutes(float realSeconds)
    {
        if (_dayNightCycle == null)
        {
            return realSeconds / 60f;
        }

        return _dayNightCycle.RealSecondsToGameMinutes(realSeconds);
    }

    private float GameMinutesToRealSeconds(float gameMinutes)
    {
        if (_dayNightCycle == null)
        {
            return gameMinutes * 60f;
        }

        return _dayNightCycle.GameMinutesToRealSeconds(gameMinutes);
    }

    private static string FormatMinutes(float gameMinutes)
    {
        int totalMinutes = Mathf.CeilToInt(Mathf.Max(0f, gameMinutes));
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        if (hours > 0)
        {
            return $"{hours} ч {minutes:00} мин";
        }

        return $"{minutes} мин";
    }
}