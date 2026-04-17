using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemCellView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _countText;
    //[SerializeField] private GameObject _selectionFrame;

    private int _slotIndex;
    private Action<int> _onClicked;

    public void Bind(InventorySlot slot, int slotIndex, bool isSelected, Action<int> onClicked)
    {
        _slotIndex = slotIndex;
        _onClicked = onClicked;

        if (slot != null && slot.Item != null)
        {
            _iconImage.enabled = slot.Item.Icon != null;
            _iconImage.sprite = slot.Item.Icon;

            bool showCount = slot.Count > 1;
            _countText.gameObject.SetActive(showCount);
            _countText.text = slot.Count.ToString();
        }
        else
        {
            _iconImage.enabled = false;
            _iconImage.sprite = null;
            _countText.gameObject.SetActive(false);
        }

        //_selectionFrame.SetActive(isSelected);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(HandleClick);
    }

    public void SetSelected(bool isSelected)
    {
        //_selectionFrame.SetActive(isSelected);
    }

    private void HandleClick()
    {
        _onClicked?.Invoke(_slotIndex);
    }
}