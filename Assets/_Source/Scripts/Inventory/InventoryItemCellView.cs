using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemCellView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private TMP_Text _durabilityText;
    [SerializeField] private Image _durabilityIcon;
    [SerializeField] private TMP_Text _weightText;
    [SerializeField] private CanvasGroup _selectionFrame;

    private int _slotIndex;
    private Action<int> _onClicked;

    private void OnDisable()
    {
        _iconImage.rectTransform.localScale = Vector3.one;
        _selectionFrame.alpha = 0.0f;
    }

    public void Bind(InventorySlot slot, int slotIndex, bool isSelected, Action<int> onClicked)
    {
        _slotIndex = slotIndex;
        _onClicked = onClicked;

        if (slot != null && slot.Item != null)
        {
            if (_iconImage != null)
            {
                _iconImage.enabled = slot.Item.Icon != null;
                _iconImage.sprite = slot.Item.Icon;
            }

            if (_countText != null)
            {
                string countLabel = InventoryDisplayFormatter.FormatCellCount(slot);
                bool showCount = !string.IsNullOrWhiteSpace(countLabel);

                _countText.gameObject.SetActive(showCount);
                _countText.text = countLabel;
            }

            if (_durabilityText != null)
            {
                bool showDurability = slot.Item.UsesDurability;
                _durabilityText.gameObject.SetActive(showDurability);
                _durabilityText.text = showDurability ? InventoryDisplayFormatter.FormatDurabilityShort(slot) : string.Empty;

                Utils.SetDurabilityColor(slot, _durabilityText, _durabilityIcon);
            }

            if (_weightText != null)
            {
                _weightText.text = InventoryDisplayFormatter.TryGetWeightText(slot, out string weightText) ? weightText : string.Empty;
            }
        }
        else
        {
            if (_iconImage != null)
            {
                _iconImage.enabled = false;
                _iconImage.sprite = null;
            }

            if (_countText != null)
            {
                _countText.gameObject.SetActive(false);
                _countText.text = string.Empty;
            }

            if (_durabilityText != null)
            {
                _durabilityText.gameObject.SetActive(false);
                _durabilityText.text = string.Empty;
            }
        }

        SetSelected(isSelected);

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(HandleClick);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _iconImage.rectTransform.DOScale(1.15f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _iconImage.rectTransform.DOScale(1.0f, 0.2f).SetEase(Ease.OutBack);
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            _selectionFrame.DOFade(1, 0.2f).SetEase(Ease.OutBack);
        }
        else
        {
            _selectionFrame.DOFade(0, 0.2f).SetEase(Ease.OutBack);
        }
    }

    private void HandleClick()
    {
        _onClicked?.Invoke(_slotIndex);
    }
}