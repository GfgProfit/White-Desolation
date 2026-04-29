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

    private float _timeOfDayMinutes;
    private int _lastReportedWholeMinute = -1;
    private bool _isRunning;
    private float _nextEnvironmentUpdateTime;

    private void Awake()
    {
        CurrentDay = DayNightTimeMath.ClampDay(_startDay);
        _timeOfDayMinutes = DayNightTimeMath.ToTimeOfDayMinutes(_startHour, _startMinute);
        _isRunning = _autoStart;

        if (_sunLight != null)
        {
            RenderSettings.sun = _sunLight;
        }

        if (_skyboxMaterial == null)
        {
            _skyboxMaterial = RenderSettings.skybox;
        }

        if (_moonLight != null)
        {
            _moonLight.color = _moonLightColor;
        }

        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);
        ApplyVisuals();
    }

    private void Update()
    {
        if (_isRunning)
        {
            AdvanceTime(Time.deltaTime);
        }

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
        hour = DayNightTimeMath.ClampHour(hour);
        minute = DayNightTimeMath.ClampMinute(minute);

        _timeOfDayMinutes = DayNightTimeMath.ToTimeOfDayMinutes(hour, minute);
        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void SetDay(int day)
    {
        CurrentDay = DayNightTimeMath.ClampDay(day);
        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void SetDateTime(int day, int hour, int minute)
    {
        CurrentDay = DayNightTimeMath.ClampDay(day);
        hour = DayNightTimeMath.ClampHour(hour);
        minute = DayNightTimeMath.ClampMinute(minute);

        _timeOfDayMinutes = DayNightTimeMath.ToTimeOfDayMinutes(hour, minute);
        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();
        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public string GetFormattedTime()
    {
        return DayNightTimeMath.FormatTime(_timeOfDayMinutes);
    }

    public float RealSecondsToGameMinutes(float realSeconds)
    {
        return DayNightTimeMath.RealSecondsToGameMinutes(realSeconds, _realSecondsPerGameDay);
    }

    public float GameMinutesToRealSeconds(float gameMinutes)
    {
        return DayNightTimeMath.GameMinutesToRealSeconds(gameMinutes, _realSecondsPerGameDay);
    }

    private void AdvanceTime(float deltaTime)
    {
        float gameMinutesPerSecond = DayNightTimeMath.GetGameMinutesPerRealSecond(_realSecondsPerGameDay);

        if (gameMinutesPerSecond <= 0f)
        {
            return;
        }

        _timeOfDayMinutes += gameMinutesPerSecond * deltaTime;

        while (_timeOfDayMinutes >= DayNightTimeMath.MinutesPerDay)
        {
            _timeOfDayMinutes -= DayNightTimeMath.MinutesPerDay;
            CurrentDay++;
            OnDayChanged?.Invoke(CurrentDay);
        }

        while (_timeOfDayMinutes < 0f)
        {
            _timeOfDayMinutes += DayNightTimeMath.MinutesPerDay;
            CurrentDay = DayNightTimeMath.ClampDay(CurrentDay - 1);
            OnDayChanged?.Invoke(CurrentDay);
        }
    }

    private void ReportMinuteChangeIfNeeded()
    {
        int wholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        if (wholeMinute == _lastReportedWholeMinute)
        {
            return;
        }

        _lastReportedWholeMinute = wholeMinute;
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    private void ApplyVisuals()
    {
        float t = NormalizedTimeOfDay;

        float dayFactor = DayNightBlendMath.EvaluateDayFactor(t, _sunriseStart, _sunriseEnd, _sunsetStart, _sunsetEnd);
        float nightFactor = DayNightBlendMath.EvaluateNightFactor(dayFactor);

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
        {
            return;
        }

        float moonAngle = t * 360f;
        _moonLight.transform.rotation =
            Quaternion.Euler(_moonRotationOffset + new Vector3(moonAngle, 0f, 0f));

        _moonLight.color = _moonLightColor;
        _moonLight.intensity = _moonMaxIntensity * Mathf.Clamp01(nightFactor);

        bool enabled = _moonLight.intensity > 0.001f;
        if (_moonLight.enabled != enabled)
        {
            _moonLight.enabled = enabled;
        }
    }

    private void UpdateMainDirectionalLight(float dayFactor, float nightFactor)
    {
        if (!_switchMainLightBetweenSunAndMoon)
        {
            return;
        }

        if (_sunLight == null && _moonLight == null)
        {
            return;
        }

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
        {
            return;
        }

        if (skybox.HasProperty("_CubemapTransition"))
        {
            skybox.SetFloat("_CubemapTransition", Mathf.Clamp01(nightFactor));
        }

        if (_controlSkyboxTint && skybox.HasProperty("_TintColor"))
        {
            skybox.SetColor("_TintColor", _skyboxTintGradient.Evaluate(t));
        }

        if (_controlSkyboxExposure && skybox.HasProperty("_Exposure"))
        {
            skybox.SetFloat("_Exposure", _skyboxExposureCurve.Evaluate(t));
        }

        if (_updateEnvironmentLighting && Time.unscaledTime >= _nextEnvironmentUpdateTime)
        {
            DynamicGI.UpdateEnvironment();
            _nextEnvironmentUpdateTime = Time.unscaledTime + _environmentUpdateInterval;
        }
    }

    private void ApplySun(float t, float dayFactor)
    {
        if (_sunLight == null)
        {
            return;
        }

        float sunAngle = t * 360f;

        _sunLight.transform.rotation = Quaternion.Euler(_sunRotationOffset + new Vector3(sunAngle, 0f, 0f));
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
        {
            return;
        }

        RenderSettings.fogColor = _fogColorGradient.Evaluate(t);
        RenderSettings.fogDensity = _fogDensityCurve.Evaluate(t);
    }

    public void SetTimeOfDayMinutes(float timeOfDayMinutes)
    {
        _timeOfDayMinutes = DayNightTimeMath.RepeatTimeOfDayMinutes(timeOfDayMinutes);
        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }

    public void SetRunning(bool running)
    {
        _isRunning = running;
    }

    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.DayNight.HasData = true;
        saveData.DayNight.Day = CurrentDay;
        saveData.DayNight.TimeOfDayMinutes = _timeOfDayMinutes;
        saveData.DayNight.IsRunning = _isRunning;
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.DayNight == null || !saveData.DayNight.HasData)
        {
            return;
        }

        CurrentDay = DayNightTimeMath.ClampDay(saveData.DayNight.Day);
        _timeOfDayMinutes = DayNightTimeMath.RepeatTimeOfDayMinutes(saveData.DayNight.TimeOfDayMinutes);
        _isRunning = saveData.DayNight.IsRunning;
        _lastReportedWholeMinute = DayNightTimeMath.GetWholeMinute(_timeOfDayMinutes);

        ApplyVisuals();

        OnDayChanged?.Invoke(CurrentDay);
        OnTimeChanged?.Invoke(CurrentDay, CurrentHour, CurrentMinute);
    }
}