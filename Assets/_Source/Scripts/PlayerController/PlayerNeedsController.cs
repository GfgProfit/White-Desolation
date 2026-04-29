using UnityEngine;
using UnityEngine.UI;

public class PlayerNeedsController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _temperatureFill;
    [SerializeField] private Image _fatigueFill;
    [SerializeField] private Image _thirstFill;
    [SerializeField] private Image _hungerFill;

    [Header("Maximum Values")]
    [SerializeField, Min(0.01f)] private float _maxTemperature = 100f;
    [SerializeField, Min(0.01f)] private float _maxFatigue = 100f;
    [SerializeField, Min(0.01f)] private float _maxThirst = 0.75f;
    [SerializeField, Min(0.01f)] private float _maxHunger = 2500f;

    [Header("Start Values")]
    [SerializeField] private float _startTemperature = 100f;
    [SerializeField] private float _startFatigue = 100f;
    [SerializeField] private float _startThirst = 0.75f;
    [SerializeField] private float _startHunger = 2500f;

    [Header("Base Drain Per Second")]
    [Tooltip("Если > 0, персонаж согревается. Если < 0, остывает.")]
    [SerializeField] private float _temperatureDeltaPerSecond = 0f;
    [SerializeField, Min(0f)] private float _fatigueDrainPerSecond = 0.1f;
    [SerializeField, Min(0f)] private float _thirstDrainPerSecond = 0.0005f;
    [SerializeField, Min(0f)] private float _hungerDrainPerSecond = 0.15f;

    [Header("Fatigue Multipliers")]
    [SerializeField] private float _fatigueIdleMultiplier = 1f;
    [SerializeField] private float _fatigueWalkMultiplier = 2f;
    [SerializeField] private float _fatigueRunMultiplier = 3f;

    [Header("Thirst Multipliers")]
    [SerializeField] private float _thirstIdleMultiplier = 1f;
    [SerializeField] private float _thirstWalkMultiplier = 2f;
    [SerializeField] private float _thirstRunMultiplier = 2f;

    [Header("Hunger Multipliers")]
    [SerializeField] private float _hungerIdleMultiplier = 1f;
    [SerializeField] private float _hungerWalkMultiplier = 1f;
    [SerializeField] private float _hungerRunMultiplier = 2f;

    public float MaxTemperature => _maxTemperature;
    public float MaxFatigue => _maxFatigue;
    public float MaxThirst => _maxThirst;
    public float MaxHunger => _maxHunger;

    public PlayerTemperatureState TemperatureState
    {
        get
        {
            if (_temperature <= 0f)
            {
                return PlayerTemperatureState.FatalHypothermia;
            }

            if (_temperature <= 25f)
            {
                return PlayerTemperatureState.Hypothermia;
            }

            if (_temperature <= 50f)
            {
                return PlayerTemperatureState.Cold;
            }

            if (_temperature <= 75f)
            {
                return PlayerTemperatureState.Cool;
            }

            return PlayerTemperatureState.Warm;
        }
    }

    public bool IsFatalHypothermia => _temperature <= 0f;
    public bool IsHypothermia => _temperature > 0f && _temperature <= 25f;
    public bool IsCold => _temperature > 25f && _temperature <= 50f;
    public bool IsCool => _temperature > 50f && _temperature <= 75f;
    public bool IsWarm => _temperature > 75f;

    public float MissingThirst => Mathf.Max(0f, _maxThirst - _thirst);
    public float MissingHunger => Mathf.Max(0f, _maxHunger - _hunger);

    public float Temperature => _temperature;
    public float Fatigue => _fatigue;
    public float Thirst => _thirst;
    public float Hunger => _hunger;

    public float TemperatureNormalized => Mathf.Clamp01(_temperature / _maxTemperature);
    public float FatigueNormalized => Mathf.Clamp01(_fatigue / _maxFatigue);
    public float ThirstNormalized => Mathf.Clamp01(_thirst / _maxThirst);
    public float HungerNormalized => Mathf.Clamp01(_hunger / _maxHunger);

    private PlayerLocomotionState _locomotionState = PlayerLocomotionState.Idle;

    private float _temperature;
    private float _fatigue;
    private float _thirst;
    private float _hunger;

    private PlayerNeedsPresenter _presenter;

    private void Awake()
    {
        _presenter = new PlayerNeedsPresenter(_temperatureFill, _fatigueFill, _thirstFill, _hungerFill);

        _temperature = Mathf.Clamp(_startTemperature, 0f, _maxTemperature);
        _fatigue = Mathf.Clamp(_startFatigue, 0f, _maxFatigue);
        _thirst = Mathf.Clamp(_startThirst, 0f, _maxThirst);
        _hunger = Mathf.Clamp(_startHunger, 0f, _maxHunger);

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
        _temperature = Mathf.Clamp(_temperature + delta, 0f, _maxTemperature);
        RefreshUI();
    }

    public void AddFatigue(float delta)
    {
        _fatigue = Mathf.Clamp(_fatigue + delta, 0f, _maxFatigue);
        RefreshUI();
    }

    public void AddThirst(float delta)
    {
        _thirst = Mathf.Clamp(_thirst + delta, 0f, _maxThirst);
        RefreshUI();
    }

    public void AddHunger(float delta)
    {
        _hunger = Mathf.Clamp(_hunger + delta, 0f, _maxHunger);
        RefreshUI();
    }

    public float RestoreThirstUpTo(float availableHydration)
    {
        if (availableHydration <= 0f)
        {
            return 0f;
        }

        float restored = Mathf.Min(MissingThirst, availableHydration);
        if (restored <= 0f)
        {
            return 0f;
        }

        AddThirst(restored);
        return restored;
    }

    public float RestoreHungerUpTo(float availableCalories)
    {
        if (availableCalories <= 0f)
        {
            return 0f;
        }

        float restored = Mathf.Min(MissingHunger, availableCalories);

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

    private void TickTemperature(float dt)
    {
        if (Mathf.Approximately(_temperatureDeltaPerSecond, 0f))
        {
            return;
        }

        _temperature = Mathf.Clamp(_temperature + (_temperatureDeltaPerSecond * dt), 0f, _maxTemperature);
    }

    private void TickFatigue(float dt)
    {
        float multiplier = _locomotionState switch
        {
            PlayerLocomotionState.Idle => _fatigueIdleMultiplier,
            PlayerLocomotionState.Walking => _fatigueWalkMultiplier,
            PlayerLocomotionState.Running => _fatigueRunMultiplier,
            _ => 1f
        };

        _fatigue = Mathf.Clamp(_fatigue - (_fatigueDrainPerSecond * multiplier * dt), 0f, _maxFatigue);
    }

    private void TickThirst(float dt)
    {
        float multiplier = _locomotionState switch
        {
            PlayerLocomotionState.Idle => _thirstIdleMultiplier,
            PlayerLocomotionState.Walking => _thirstWalkMultiplier,
            PlayerLocomotionState.Running => _thirstRunMultiplier,
            _ => 1f
        };

        _thirst = Mathf.Clamp(_thirst - (_thirstDrainPerSecond * multiplier * dt), 0f, _maxThirst);
    }

    private void TickHunger(float dt)
    {
        float multiplier = _locomotionState switch
        {
            PlayerLocomotionState.Idle => _hungerIdleMultiplier,
            PlayerLocomotionState.Walking => _hungerWalkMultiplier,
            PlayerLocomotionState.Running => _hungerRunMultiplier,
            _ => 1f
        };

        _hunger = Mathf.Clamp(_hunger - (_hungerDrainPerSecond * multiplier * dt), 0f, _maxHunger);
    }

    private void RefreshUI()
    {
        _presenter?.Refresh(TemperatureNormalized, FatigueNormalized, ThirstNormalized, HungerNormalized);
    }

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

        _temperature = Mathf.Clamp(saveData.PlayerNeeds.Temperature, 0f, _maxTemperature);
        _fatigue = Mathf.Clamp(saveData.PlayerNeeds.Fatigue, 0f, _maxFatigue);
        _thirst = Mathf.Clamp(saveData.PlayerNeeds.Thirst, 0f, _maxThirst);
        _hunger = Mathf.Clamp(saveData.PlayerNeeds.Hunger, 0f, _maxHunger);

        RefreshUI();
    }
}