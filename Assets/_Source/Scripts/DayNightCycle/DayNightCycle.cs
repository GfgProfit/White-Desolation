using System;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
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
    [SerializeField]
    private AnimationCurve _skyboxExposureCurve = new(
        new Keyframe(0.00f, 0.45f),
        new Keyframe(0.22f, 0.55f),
        new Keyframe(0.28f, 0.95f),
        new Keyframe(0.50f, 1.00f),
        new Keyframe(0.72f, 0.95f),
        new Keyframe(0.80f, 0.55f),
        new Keyframe(1.00f, 0.45f)
    );

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
    [SerializeField, Range(0f, 1f)] private float _sunriseEnd = 0.28f;   // ~06:43
    [SerializeField, Range(0f, 1f)] private float _sunsetStart = 0.72f;  // ~17:17
    [SerializeField, Range(0f, 1f)] private float _sunsetEnd = 0.80f;    // ~19:12

    [Header("Ambient")]
    [SerializeField] private Gradient _ambientColorGradient;
    [SerializeField]
    private AnimationCurve _ambientIntensityCurve = new(
        new Keyframe(0.00f, 0.20f),
        new Keyframe(0.25f, 0.35f),
        new Keyframe(0.50f, 1.00f),
        new Keyframe(0.75f, 0.35f),
        new Keyframe(1.00f, 0.20f)
    );

    [Header("Fog")]
    [SerializeField] private bool _controlFog = true;
    [SerializeField] private Gradient _fogColorGradient;
    [SerializeField]
    private AnimationCurve _fogDensityCurve = new(
        new Keyframe(0.00f, 0.015f),
        new Keyframe(0.25f, 0.010f),
        new Keyframe(0.50f, 0.004f),
        new Keyframe(0.75f, 0.010f),
        new Keyframe(1.00f, 0.015f)
    );

    public int CurrentDay { get; private set; }
    public int CurrentHour => Mathf.FloorToInt(_timeOfDayMinutes / 60f) % 24;
    public int CurrentMinute => Mathf.FloorToInt(_timeOfDayMinutes % 60f);
    public float TimeOfDayMinutes => _timeOfDayMinutes;
    public float NormalizedTimeOfDay => _timeOfDayMinutes / 1440f;
    public float DayFactor => EvaluateDayFactor(NormalizedTimeOfDay);
    public float NightFactor => 1f - DayFactor;

    public event Action<int, int, int> OnTimeChanged;
    public event Action<int> OnDayChanged;

    private float _timeOfDayMinutes;
    private int _lastReportedWholeMinute = -1;
    private bool _isRunning;
    private static readonly int CubemapTransitionId = Shader.PropertyToID("_CubemapTransition");
    private float _nextEnvironmentUpdateTime;

    private void Awake()
    {
        CurrentDay = Mathf.Max(1, _startDay);
        _timeOfDayMinutes = Mathf.Clamp(_startHour, 0, 23) * 60f + Mathf.Clamp(_startMinute, 0, 59);
        _isRunning = _autoStart;

        if (_sunLight != null)
            RenderSettings.sun = _sunLight;

        if (_skyboxMaterial == null)
            _skyboxMaterial = RenderSettings.skybox;

        if (_moonLight != null)
            _moonLight.color = _moonLightColor;

        _lastReportedWholeMinute = Mathf.FloorToInt(_timeOfDayMinutes);
        ApplyVisuals();
    }

    private void Update()
    {
        if (_isRunning)
            AdvanceTime(Time.deltaTime);

        ApplyVisuals();
        ReportMinuteChangeIfNeeded();
    }

    public void StartTime()
    {
        _isRunning = true;
    }

    public void StopTime()
    {
        _isRunning = false;
    }

    public void SetTime(int hour, int minute)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);

        _timeOfDayMinutes = hour * 60f + minute;
        _lastReportedWholeMinute = Mathf.FloorToInt(_timeOfDayMinutes);

        ApplyVisuals();
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void SetDay(int day)
    {
        CurrentDay = Mathf.Max(1, day);
        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void SetDateTime(int day, int hour, int minute)
    {
        CurrentDay = Mathf.Max(1, day);
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);

        _timeOfDayMinutes = hour * 60f + minute;
        _lastReportedWholeMinute = Mathf.FloorToInt(_timeOfDayMinutes);

        ApplyVisuals();
        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public string GetFormattedTime()
    {
        return $"{CurrentHour:00}:{CurrentMinute:00}";
    }

    private void AdvanceTime(float deltaTime)
    {
        if (_realSecondsPerGameDay <= 0.01f)
            return;

        float gameMinutesPerSecond = 1440f / _realSecondsPerGameDay;
        _timeOfDayMinutes += gameMinutesPerSecond * deltaTime;

        while (_timeOfDayMinutes >= 1440f)
        {
            _timeOfDayMinutes -= 1440f;
            CurrentDay++;
            OnDayChanged?.Invoke(CurrentDay);
        }

        while (_timeOfDayMinutes < 0f)
        {
            _timeOfDayMinutes += 1440f;
            CurrentDay = Mathf.Max(1, CurrentDay - 1);
            OnDayChanged?.Invoke(CurrentDay);
        }
    }

    private void ReportMinuteChangeIfNeeded()
    {
        int wholeMinute = Mathf.FloorToInt(_timeOfDayMinutes);

        if (wholeMinute == _lastReportedWholeMinute)
            return;

        _lastReportedWholeMinute = wholeMinute;
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    private void ApplyVisuals()
    {
        float t = NormalizedTimeOfDay;

        float dayFactor = EvaluateDayFactor(t);
        float nightFactor = 1f - dayFactor;

        ApplySun(t, dayFactor);
        ApplyMoon(t, nightFactor);
        ApplyAmbient(t);
        ApplyFog(t);
        ApplySkybox(t, nightFactor);
        UpdateMainDirectionalLight(dayFactor, nightFactor);
    }

    private void ApplyMoon(float t, float nightFactor)
    {
        if (_moonLight == null)
            return;

        float moonAngle = t * 360f;
        _moonLight.transform.rotation =
            Quaternion.Euler(_moonRotationOffset + new Vector3(moonAngle, 0f, 0f));

        _moonLight.color = _moonLightColor;
        _moonLight.intensity = _moonMaxIntensity * Mathf.Clamp01(nightFactor);

        bool enabled = _moonLight.intensity > 0.001f;
        if (_moonLight.enabled != enabled)
            _moonLight.enabled = enabled;
    }

    private void UpdateMainDirectionalLight(float dayFactor, float nightFactor)
    {
        if (!_switchMainLightBetweenSunAndMoon)
            return;

        if (_sunLight == null && _moonLight == null)
            return;

        if (_sunLight != null && _moonLight == null)
        {
            RenderSettings.sun = _sunLight;
            return;
        }

        if (_moonLight != null && _sunLight == null)
        {
            RenderSettings.sun = _moonLight;
            return;
        }

        RenderSettings.sun = dayFactor >= nightFactor ? _sunLight : _moonLight;
    }

    private void ApplySkybox(float t, float nightFactor)
    {
        Material skybox = _skyboxMaterial != null ? _skyboxMaterial : RenderSettings.skybox;
        if (skybox == null)
            return;

        if (skybox.HasProperty("_CubemapTransition"))
            skybox.SetFloat("_CubemapTransition", Mathf.Clamp01(nightFactor));

        if (_controlSkyboxTint && skybox.HasProperty("_TintColor"))
            skybox.SetColor("_TintColor", _skyboxTintGradient.Evaluate(t));

        if (_controlSkyboxExposure && skybox.HasProperty("_Exposure"))
            skybox.SetFloat("_Exposure", _skyboxExposureCurve.Evaluate(t));

        if (_updateEnvironmentLighting && Time.unscaledTime >= _nextEnvironmentUpdateTime)
        {
            DynamicGI.UpdateEnvironment();
            _nextEnvironmentUpdateTime = Time.unscaledTime + _environmentUpdateInterval;
        }
    }

    private void ApplySun(float t, float dayFactor)
    {
        if (_sunLight == null)
            return;

        float sunAngle = t * 360f;
        _sunLight.transform.rotation =
            Quaternion.Euler(_sunRotationOffset + new Vector3(sunAngle, 0f, 0f));

        _sunLight.intensity = _sunMaxIntensity * dayFactor;
    }

    private void ApplyAmbient(float t)
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = _ambientColorGradient.Evaluate(t);
        RenderSettings.ambientIntensity = _ambientIntensityCurve.Evaluate(t);
    }

    private void ApplyFog(float t)
    {
        if (!_controlFog)
            return;

        RenderSettings.fogColor = _fogColorGradient.Evaluate(t);
        RenderSettings.fogDensity = _fogDensityCurve.Evaluate(t);
    }

    private float EvaluateDayFactor(float t)
    {
        float sunrise = SmoothStep01(_sunriseStart, _sunriseEnd, t);
        float sunset = 1f - SmoothStep01(_sunsetStart, _sunsetEnd, t);
        return Mathf.Clamp01(sunrise * sunset);
    }

    private static float SmoothStep01(float start, float end, float value)
    {
        if (Mathf.Approximately(start, end))
            return value >= end ? 1f : 0f;

        float x = Mathf.InverseLerp(start, end, value);
        return x * x * (3f - 2f * x);
    }
}