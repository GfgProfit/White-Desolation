using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveId))]
public sealed class FireSourceInteractable : MonoBehaviour, IInteractable, IInteractHoverInfo, ISaveable
{
    [Header("Display")]
    [SerializeField] private string _displayName = "Печка";

    [Header("Runtime")]
    [SerializeField] private bool _isBurning;
    [SerializeField, Min(0f)] private float _remainingBurnGameMinutes;

    [Header("Temperature")]
    [SerializeField] private float _temperatureCelsius = 0f;

    [Header("Save")]
    [SerializeField] private SaveId _saveId;

    [Inject] private IFireSourceInteractionHandler _interactionHandler = null;
    [Inject] private IGameTimeConverter _gameTimeConverter = null;

    public string SaveId => _saveId != null ? _saveId.Id : string.Empty;

    public string DisplayName => _displayName;
    public bool IsBurning => _isBurning;
    public float RemainingBurnSeconds => GameMinutesToRealSeconds(_remainingBurnGameMinutes);
    public float RemainingBurnMinutes => _remainingBurnGameMinutes;
    public float TemperatureCelsius => _temperatureCelsius;

    private void Reset()
    {
        _saveId = GetComponent<SaveId>();
    }

    private void Awake()
    {
        if (_saveId == null)
        {
            _saveId = GetComponent<SaveId>();
        }
    }

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
        if (_interactionHandler == null)
        {
            Debug.LogWarning("[FireSource] Fire interaction handler is missing.");
            return;
        }

        _interactionHandler.InteractWith(this);
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
        InteractionHoverInfo info = new()
        {
            InteractionText = _displayName
        };

        if (!_isBurning)
        {
            return info;
        }

        info.TimeText = FormatMinutes(_remainingBurnGameMinutes);
        info.TemperatureText = $"{_temperatureCelsius:0.#} °C";

        return info;
    }

    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SaveId))
        {
            Debug.LogWarning($"[FireSource] Cannot save without SaveId: {name}");
            return;
        }

        RemoveOldState(saveData.FireSources, SaveId);

        saveData.FireSources.Add(new FireSourceSaveData
        {
            SaveId = SaveId,
            IsBurning = _isBurning,
            RemainingBurnGameMinutes = _remainingBurnGameMinutes
        });
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.FireSources == null)
        {
            return;
        }

        FireSourceSaveData state = FindState(saveData.FireSources, SaveId);

        if (state == null)
        {
            return;
        }

        _isBurning = state.IsBurning;
        _remainingBurnGameMinutes = Mathf.Max(0f, state.RemainingBurnGameMinutes);

        if (_remainingBurnGameMinutes <= 0f)
        {
            _isBurning = false;
        }
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

    private float RealSecondsToGameMinutes(float realSeconds)
    {
        if (_gameTimeConverter == null)
        {
            return realSeconds / 60f;
        }

        return _gameTimeConverter.RealSecondsToGameMinutes(realSeconds);
    }

    private float GameMinutesToRealSeconds(float gameMinutes)
    {
        if (_gameTimeConverter == null)
        {
            return gameMinutes * 60f;
        }

        return _gameTimeConverter.GameMinutesToRealSeconds(gameMinutes);
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

    private static FireSourceSaveData FindState(List<FireSourceSaveData> states, string saveId)
    {
        if (states == null || string.IsNullOrWhiteSpace(saveId))
        {
            return null;
        }

        for (int i = 0; i < states.Count; i++)
        {
            FireSourceSaveData state = states[i];

            if (state != null && state.SaveId == saveId)
            {
                return state;
            }
        }

        return null;
    }

    private static void RemoveOldState(List<FireSourceSaveData> states, string saveId)
    {
        if (states == null || string.IsNullOrWhiteSpace(saveId))
        {
            return;
        }

        for (int i = states.Count - 1; i >= 0; i--)
        {
            if (states[i] != null && states[i].SaveId == saveId)
            {
                states.RemoveAt(i);
            }
        }
    }
}
