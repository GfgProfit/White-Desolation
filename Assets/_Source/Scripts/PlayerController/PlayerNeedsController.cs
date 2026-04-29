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

    public PlayerTemperatureState TemperatureState => PlayerTemperatureStateResolver.Resolve(_temperature);
    public bool IsFatalHypothermia => PlayerTemperatureStateResolver.IsFatalHypothermia(_temperature);
    public bool IsHypothermia => PlayerTemperatureStateResolver.IsHypothermia(_temperature);
    public bool IsCold => PlayerTemperatureStateResolver.IsCold(_temperature);
    public bool IsCool => PlayerTemperatureStateResolver.IsCool(_temperature);
    public bool IsWarm => PlayerTemperatureStateResolver.IsWarm(_temperature);

    public float MissingThirst => PlayerNeedValueMath.Missing(_thirst, _maxThirst);
    public float MissingHunger => PlayerNeedValueMath.Missing(_hunger, _maxHunger);

    public float Temperature => _temperature;
    public float Fatigue => _fatigue;
    public float Thirst => _thirst;
    public float Hunger => _hunger;

    public float TemperatureNormalized => PlayerNeedValueMath.Normalize(_temperature, _maxTemperature);
    public float FatigueNormalized => PlayerNeedValueMath.Normalize(_fatigue, _maxFatigue);
    public float ThirstNormalized => PlayerNeedValueMath.Normalize(_thirst, _maxThirst);
    public float HungerNormalized => PlayerNeedValueMath.Normalize(_hunger, _maxHunger);

    private PlayerLocomotionState _locomotionState = PlayerLocomotionState.Idle;

    private float _temperature;
    private float _fatigue;
    private float _thirst;
    private float _hunger;

    private PlayerNeedsPresenter _presenter;

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

        _temperature = PlayerNeedValueMath.Clamp(saveData.PlayerNeeds.Temperature, _maxTemperature);
        _fatigue = PlayerNeedValueMath.Clamp(saveData.PlayerNeeds.Fatigue, _maxFatigue);
        _thirst = PlayerNeedValueMath.Clamp(saveData.PlayerNeeds.Thirst, _maxThirst);
        _hunger = PlayerNeedValueMath.Clamp(saveData.PlayerNeeds.Hunger, _maxHunger);

        RefreshUI();
    }
}