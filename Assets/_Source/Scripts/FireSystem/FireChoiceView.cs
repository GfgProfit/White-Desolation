using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireChoiceView : MonoBehaviour
{
    [SerializeField] private CustomButton _previousButton;
    [SerializeField] private CustomButton _nextButton;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _emptyImage;

    public void Bind(Action onPrevious, Action onNext)
    {
        _previousButton.OnClick.RemoveAllListeners();
        _previousButton.OnClick.AddListener(() => onPrevious?.Invoke());

        _nextButton.OnClick.RemoveAllListeners();
        _nextButton.OnClick.AddListener(() => onNext?.Invoke());
    }

    public void Refresh(ItemData itemData, string amountText)
    {
        _nameText.text = itemData != null ? itemData.DisplayName : "ни одного";

        bool hasAmountText = itemData != null && !string.IsNullOrWhiteSpace(amountText);

        _countText.gameObject.SetActive(hasAmountText);
        _countText.text = hasAmountText ? amountText : string.Empty;

        _iconImage.sprite = itemData == null ? null : itemData.Icon;
        _iconImage.enabled = itemData != null;
        _emptyImage.enabled = itemData == null;
    }
}