using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireOperationListItemView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private GameObject _amountControlsRoot;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private Button _amountDecreaseButton;
    [SerializeField] private Button _amountIncreaseButton;
    [SerializeField, Range(0f, 1f)] private float _activeAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float _inactiveAlpha = 0.2f;

    public void Refresh(FireBurningOperationListEntry entry, bool selected, Action onClick, string amountText, bool canDecreaseAmount, bool canIncreaseAmount, Action onDecreaseAmount, Action onIncreaseAmount)
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = entry.Icon;
            _iconImage.enabled = entry.Icon != null;
        }

        if (_nameText != null)
        {
            _nameText.text = entry.Name;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = selected && entry.Interactable ? _activeAlpha : _inactiveAlpha;
        }

        RefreshAmountControls(entry.SupportsAmountControls && selected && entry.Interactable, amountText, canDecreaseAmount, canIncreaseAmount, onDecreaseAmount, onIncreaseAmount);

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.interactable = entry.Interactable;

            if (entry.Interactable)
            {
                _button.onClick.AddListener(() => onClick?.Invoke());
            }
        }
    }

    private void RefreshAmountControls(bool visible, string amountText, bool canDecreaseAmount, bool canIncreaseAmount, Action onDecreaseAmount, Action onIncreaseAmount)
    {
        if (_amountControlsRoot != null)
        {
            _amountControlsRoot.SetActive(visible);
        }

        if (_amountText != null)
        {
            _amountText.text = amountText;
        }

        BindAmountButton(_amountDecreaseButton, visible && canDecreaseAmount, onDecreaseAmount);
        BindAmountButton(_amountIncreaseButton, visible && canIncreaseAmount, onIncreaseAmount);
    }

    private static void BindAmountButton(Button button, bool interactable, Action action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.interactable = interactable;

        if (interactable)
        {
            button.onClick.AddListener(() => action?.Invoke());
        }
    }
}
