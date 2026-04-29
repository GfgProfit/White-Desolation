using UnityEngine;
using UnityEngine.UI;

public partial class PlayerNeedsController : MonoBehaviour, IPlayerNeeds, IGlobalSaveable
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
}
