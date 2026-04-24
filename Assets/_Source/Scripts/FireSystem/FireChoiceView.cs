using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireChoiceView : MonoBehaviour
{
    [SerializeField] private CustomButton _previousButton;
    [SerializeField] private CustomButton _nextButton;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _emptyImage;

    public void Bind(Action onPrevious, Action onNext)
    {
        _previousButton.OnClick.RemoveAllListeners();
        _previousButton.OnClick.AddListener(() => onPrevious?.Invoke());

        _nextButton.OnClick.RemoveAllListeners();
        _nextButton.OnClick.AddListener(() => onNext?.Invoke());
    }

    public void Refresh(ItemData itemData)
    {
        _nameText.text = itemData == null ? "ни одного" : itemData.DisplayName;
        _iconImage.sprite = itemData == null ? null : itemData.Icon;
        _iconImage.enabled = itemData != null;
        _emptyImage.enabled = itemData == null;
    }
}