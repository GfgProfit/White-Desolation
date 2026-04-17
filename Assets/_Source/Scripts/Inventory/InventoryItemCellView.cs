using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemCellView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private TMP_Text _durabilityText;
    [SerializeField] private TMP_Text _weightText;
    //[SerializeField] private GameObject _selectionFrame;

    private int _slotIndex;
    private Action<int> _onClicked;

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

    public void SetSelected(bool isSelected)
    {
        //if (_selectionFrame != null)
        //    _selectionFrame.SetActive(isSelected);
    }

    private void HandleClick()
    {
        _onClicked?.Invoke(_slotIndex);
    }
}