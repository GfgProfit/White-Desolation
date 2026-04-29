using UnityEngine;

public partial class DayNightCycle
{
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
}