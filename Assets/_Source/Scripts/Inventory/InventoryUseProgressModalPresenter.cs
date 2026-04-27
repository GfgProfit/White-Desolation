using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventoryUseProgressModalPresenter
{
    private readonly GameObject _root;
    private readonly Image _fillImage;
    private readonly TMP_Text _text;

    public InventoryUseProgressModalPresenter(GameObject root, Image fillImage, TMP_Text text)
    {
        _root = root;
        _fillImage = fillImage;
        _text = text;
    }

    public void InitializeHidden()
    {
        HideAndReset();
    }

    public void Show(string text)
    {
        SetVisible(true);
        SetProgress(0f, text);
    }

    public void UpdateProgress(float progress01, string text)
    {
        SetProgress(progress01, text);
    }

    public void Complete(string text)
    {
        SetProgress(1f, text);
    }

    public void HideAndReset()
    {
        SetVisible(false);
        SetProgress(0f, string.Empty);
    }

    private void SetVisible(bool visible)
    {
        if (_root != null)
        {
            _root.SetActive(visible);
        }
    }

    private void SetProgress(float progress01, string text)
    {
        if (_fillImage != null)
        {
            _fillImage.fillAmount = Mathf.Clamp01(progress01);
        }

        if (_text != null)
        {
            _text.text = text ?? string.Empty;
        }
    }
}