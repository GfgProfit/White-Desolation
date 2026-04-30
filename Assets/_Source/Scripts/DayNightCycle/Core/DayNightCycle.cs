using System;
using UnityEngine;

public partial class DayNightCycle : MonoBehaviour, IGameTimeConverter, IGameTimeAdvancer, IGameTimeRunController, IGameTimeAdvanceNotifier, IGlobalSaveable
{
    [Header("Time")]
    [SerializeField, Min(1f)] private float _realSecondsPerGameDay = 1800f;
    [SerializeField, Range(1, 9999)] private int _startDay = 1;
    [SerializeField, Range(0, 23)] private int _startHour = 8;
    [SerializeField, Range(0, 59)] private int _startMinute = 0;
    [SerializeField] private bool _autoStart = true;

    [Header("Skybox")]
    [SerializeField] private Material _skyboxMaterial;
    [SerializeField] private bool _updateEnvironmentLighting = false;
    [SerializeField, Min(0.1f)] private float _environmentUpdateInterval = 1f;

    [Space]
    [SerializeField] private bool _controlSkyboxTint = true;
    [SerializeField] private Gradient _skyboxTintGradient;

    [Space]
    [SerializeField] private bool _controlSkyboxExposure = true;
    [SerializeField] private AnimationCurve _skyboxExposureCurve = new(new Keyframe(0.00f, 0.45f), new Keyframe(0.22f, 0.55f), new Keyframe(0.28f, 0.95f), new Keyframe(0.50f, 1.00f), new Keyframe(0.72f, 0.95f), new Keyframe(0.80f, 0.55f), new Keyframe(1.00f, 0.45f));

    [Header("Sun")]
    [SerializeField] private Light _sunLight;
    [SerializeField] private Vector3 _sunRotationOffset = new(-90f, 170f, 0f);
    [SerializeField, Min(0f)] private float _sunMaxIntensity = 1.0f;

    [Header("Moon")]
    [SerializeField] private Light _moonLight;
    [SerializeField] private Vector3 _moonRotationOffset = new(90f, 170f, 0f);
    [SerializeField, Min(0f)] private float _moonMaxIntensity = 0.25f;
    [SerializeField] private Color _moonLightColor = new(0.75f, 0.82f, 1f);
    [SerializeField] private bool _switchMainLightBetweenSunAndMoon = true;

    [Header("Day / Night Blend")]
    [SerializeField, Range(0f, 1f)] private float _sunriseStart = 0.20f; // ~04:48
    [SerializeField, Range(0f, 1f)] private float _sunriseEnd = 0.28f; // ~06:43
    [SerializeField, Range(0f, 1f)] private float _sunsetStart = 0.72f; // ~17:17
    [SerializeField, Range(0f, 1f)] private float _sunsetEnd = 0.80f; // ~19:12

    [Header("Ambient")]
    [SerializeField] private Gradient _ambientColorGradient;
    [SerializeField] private AnimationCurve _ambientIntensityCurve = new(new Keyframe(0.00f, 0.20f), new Keyframe(0.25f, 0.35f), new Keyframe(0.50f, 1.00f), new Keyframe(0.75f, 0.35f), new Keyframe(1.00f, 0.20f));

    [Header("Fog")]
    [SerializeField] private bool _controlFog = true;
    [SerializeField] private Gradient _fogColorGradient;
    [SerializeField] private AnimationCurve _fogDensityCurve = new(new Keyframe(0.00f, 0.015f), new Keyframe(0.25f, 0.010f), new Keyframe(0.50f, 0.004f), new Keyframe(0.75f, 0.010f), new Keyframe(1.00f, 0.015f));

    public int CurrentDay { get; private set; }
    public int CurrentHour => DayNightTimeMath.GetHour(_timeOfDayMinutes);
    public int CurrentMinute => DayNightTimeMath.GetMinute(_timeOfDayMinutes);
    public float TimeOfDayMinutes => _timeOfDayMinutes;
    public float NormalizedTimeOfDay => DayNightTimeMath.GetNormalizedTimeOfDay(_timeOfDayMinutes);
    public float DayFactor => DayNightBlendMath.EvaluateDayFactor(NormalizedTimeOfDay, _sunriseStart, _sunriseEnd, _sunsetStart, _sunsetEnd);
    public float NightFactor => DayNightBlendMath.EvaluateNightFactor(DayFactor);
    public bool IsRunning => _isRunning;

    public event Action<int, int, int> OnTimeChanged;
    public event Action<int> OnDayChanged;
    public event Action<float> OnGameMinutesAdvanced;

    private float _timeOfDayMinutes;
    private int _lastReportedWholeMinute = -1;
    private bool _isRunning;
    private float _nextEnvironmentUpdateTime;
}
