using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireProgressView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Image _fillImage;
    [SerializeField] private TMP_Text _stateText;

    public void Show(string text)
    {
        if (_root != null)
        {
            _root.SetActive(true);
        }

        SetText(text);
        SetFill(0f);
    }

    public void Hide()
    {
        if (_root != null)
        {
            _root.SetActive(false);
        }

        SetFill(0f);
    }

    public void SetFill(float value)
    {
        if (_fillImage != null)
        {
            _fillImage.fillAmount = Mathf.Clamp01(value);
        }
    }

    public void SetText(string text)
    {
        if (_stateText != null)
        {
            _stateText.text = text;
        }
    }
}