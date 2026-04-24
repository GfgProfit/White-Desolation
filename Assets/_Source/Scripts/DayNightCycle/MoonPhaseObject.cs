using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MoonPhaseObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayNightCycle _dayNightCycle;
    [SerializeField] private Light _moonLight;
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Moon Phases")]
    [SerializeField] private Sprite[] _phaseSprites;
    [SerializeField, Min(1)] private int _daysPerMoonCycle = 30;
    [SerializeField] private int _phaseOffsetDays = 0;

    [Header("Placement")]
    [SerializeField, Min(1f)] private float _distanceFromCamera = 900f;
    [SerializeField, Min(0.01f)] private float _scale = 35f;
    [SerializeField] private Vector3 _positionOffset = Vector3.zero;

    [Header("Visibility")]
    [SerializeField] private Color _moonColor = Color.white;
    [SerializeField, Range(0f, 1f)] private float _maxAlpha = 1f;
    [SerializeField] private bool _hideDuringDay = true;
    [SerializeField, Range(0f, 1f)] private float _dayFadeThreshold = 0.05f;

    private void Reset()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (_dayNightCycle == null || _moonLight == null || _spriteRenderer == null)
        {
            return;
        }

        if (_targetCamera == null)
        {
            return;
        }

        UpdateTransform(_targetCamera);
        UpdatePhase();
        UpdateVisibility();
    }

    private void UpdateTransform(Camera cam)
    {
        float distance = Mathf.Min(_distanceFromCamera, cam.farClipPlane * 0.9f);

        Vector3 moonDirection = -_moonLight.transform.forward.normalized;

        transform.position = cam.transform.position + moonDirection * distance + _positionOffset;

        Vector3 toCamera = cam.transform.position - transform.position;

        if (toCamera.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(toCamera.normalized, cam.transform.up);
        }

        transform.localScale = Vector3.one * _scale;
    }

    private void UpdatePhase()
    {
        if (_phaseSprites == null || _phaseSprites.Length == 0)
        {
            return;
        }

        int dayZeroBased = Mathf.Max(0, _dayNightCycle.CurrentDay - 1);
        int cycleDay = Mathf.FloorToInt(Mathf.Repeat(dayZeroBased + _phaseOffsetDays, _daysPerMoonCycle));

        float phase01 = cycleDay / (float)_daysPerMoonCycle;
        int phaseIndex = Mathf.Clamp(Mathf.FloorToInt(phase01 * _phaseSprites.Length), 0, _phaseSprites.Length - 1);

        Sprite targetSprite = _phaseSprites[phaseIndex];

        if (_spriteRenderer.sprite != targetSprite)
        {
            _spriteRenderer.sprite = targetSprite;
        }
    }

    private void UpdateVisibility()
    {
        float nightFactor = _dayNightCycle.NightFactor;

        float alpha = _hideDuringDay ? Mathf.Clamp01(Mathf.InverseLerp(_dayFadeThreshold, 1f, nightFactor)) : Mathf.Clamp01(nightFactor);

        Color color = _moonColor;
        color.a *= alpha * _maxAlpha;

        _spriteRenderer.color = color;
        _spriteRenderer.enabled = color.a > 0.001f;
    }
}