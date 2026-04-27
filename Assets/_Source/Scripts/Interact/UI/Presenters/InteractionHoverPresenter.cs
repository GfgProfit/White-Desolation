using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InteractionHoverPresenter
{
    private readonly GameObject _hoverRoot;
    private CanvasGroup _hoverCanvasGroup;
    private readonly TMP_Text _hoverNameText;

    private readonly Image _lineImage;

    private readonly GameObject _timeHolder;
    private TMP_Text _timeText;

    private readonly GameObject _temperatureHolder;
    private TMP_Text _temperatureText;

    private readonly GameObject _infoHolder;
    private TMP_Text _infoText;

    private readonly float _fadeDuration;

    private bool _visibleTarget;

    public InteractionHoverPresenter(GameObject hoverRoot, CanvasGroup hoverCanvasGroup, TMP_Text hoverNameText, Image lineImage, GameObject timeHolder, TMP_Text timeText, GameObject temperatureHolder, TMP_Text temperatureText, GameObject infoHolder, TMP_Text infoText, float fadeDuration)
    {
        _hoverRoot = hoverRoot;
        _hoverCanvasGroup = hoverCanvasGroup;
        _hoverNameText = hoverNameText;

        _lineImage = lineImage;

        _timeHolder = timeHolder;
        _timeText = timeText;

        _temperatureHolder = temperatureHolder;
        _temperatureText = temperatureText;

        _infoHolder = infoHolder;
        _infoText = infoText;

        _fadeDuration = Mathf.Max(0f, fadeDuration);
    }

    public void CacheReferences()
    {
        if (_hoverCanvasGroup == null && _hoverRoot != null)
        {
            _hoverCanvasGroup = _hoverRoot.GetComponent<CanvasGroup>();
        }

        if (_timeText == null && _timeHolder != null)
        {
            _timeText = _timeHolder.GetComponentInChildren<TMP_Text>(true);
        }

        if (_temperatureText == null && _temperatureHolder != null)
        {
            _temperatureText = _temperatureHolder.GetComponentInChildren<TMP_Text>(true);
        }

        if (_infoText == null && _infoHolder != null)
        {
            _infoText = _infoHolder.GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void Apply(InteractionHoverInfo info, bool instant = false)
    {
        bool hasInteractionText = info.HasInteractionText;

        if (_hoverNameText != null)
        {
            _hoverNameText.text = hasInteractionText ? info.InteractionText : string.Empty;
            _hoverNameText.gameObject.SetActive(hasInteractionText);
        }

        bool hasTime = SetHolderText(_timeHolder, _timeText, info.TimeText);
        bool hasTemperature = SetHolderText(_temperatureHolder, _temperatureText, info.TemperatureText);
        bool hasInfo = SetHolderText(_infoHolder, _infoText, info.InfoText);

        bool hasExtra = hasTime || hasTemperature || hasInfo;

        if (_lineImage != null)
        {
            _lineImage.gameObject.SetActive(hasExtra);
        }

        SetVisible(info.HasAnyText, instant);
    }

    public void UpdateFade()
    {
        if (_hoverCanvasGroup == null)
        {
            return;
        }

        float targetAlpha = _visibleTarget ? 1f : 0f;

        if (_fadeDuration <= 0f)
        {
            _hoverCanvasGroup.alpha = targetAlpha;
        }
        else
        {
            _hoverCanvasGroup.alpha = Mathf.MoveTowards(_hoverCanvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime / _fadeDuration);
        }

        if (_hoverRoot != null && !_visibleTarget && _hoverCanvasGroup.alpha <= 0f)
        {
            _hoverRoot.SetActive(false);
        }
    }

    private void SetVisible(bool visible, bool instant = false)
    {
        _visibleTarget = visible;

        if (_hoverRoot != null && visible && !_hoverRoot.activeSelf)
        {
            _hoverRoot.SetActive(true);
        }

        if (_hoverCanvasGroup == null)
        {
            if (_hoverRoot != null)
            {
                _hoverRoot.SetActive(visible);
            }

            return;
        }

        _hoverCanvasGroup.interactable = false;
        _hoverCanvasGroup.blocksRaycasts = false;

        if (!instant)
        {
            return;
        }

        _hoverCanvasGroup.alpha = visible ? 1f : 0f;

        if (_hoverRoot != null && !visible)
        {
            _hoverRoot.SetActive(false);
        }
    }

    private static bool SetHolderText(GameObject holder, TMP_Text text, string value)
    {
        bool visible = !string.IsNullOrWhiteSpace(value);

        if (text != null)
        {
            text.text = visible ? value : string.Empty;
        }

        if (holder != null)
        {
            holder.SetActive(visible);
        }

        return visible;
    }
}