using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireStartingChoiceView : MonoBehaviour
{
    [SerializeField] private CustomButton _previousButton;
    [SerializeField] private CustomButton _nextButton;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;

    public void Bind(Action onPrevious, Action onNext)
    {
        if (_previousButton != null)
        {
            _previousButton.OnClick.RemoveAllListeners();
            _previousButton.OnClick.AddListener(() => onPrevious?.Invoke());
        }

        if (_nextButton != null)
        {
            _nextButton.OnClick.RemoveAllListeners();
            _nextButton.OnClick.AddListener(() => onNext?.Invoke());
        }
    }

    public void Refresh(ItemData itemData)
    {
        if (_nameText != null)
        {
            _nameText.text = itemData != null ? itemData.DisplayName : "ни одного";
        }

        if (_icon == null)
        {
            return;
        }

        _icon.enabled = itemData != null && itemData.Icon != null;
        _icon.sprite = itemData != null ? itemData.Icon : null;
    }
}
