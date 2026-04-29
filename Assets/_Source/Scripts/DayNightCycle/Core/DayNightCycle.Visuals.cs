using UnityEngine;

public partial class DayNightCycle
{
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
        _moonLight.transform.rotation = Quaternion.Euler(_moonRotationOffset + new Vector3(moonAngle, 0f, 0f));

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
}