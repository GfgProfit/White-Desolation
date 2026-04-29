using System;
using System.Collections.Generic;
using UnityEngine.UI;

public sealed class FireBurningOperationWindowPresenter
{
    private readonly FireBurningOperationWindowView _view;
    private readonly List<FireOperationListItemView> _spawnedItems = new();
    private Action _onDecreaseAmount;
    private Action _onIncreaseAmount;

    public bool IsOpen => _view != null && _view.Root != null && _view.Root.activeSelf;

    public FireBurningOperationWindowPresenter(FireBurningOperationWindowView view)
    {
        _view = view;
    }

    public void Bind(Action onAddFuelTab, Action onCookingTab, Action onWaterTab, Action onAction, Action onClose, Action onDecreaseAmount, Action onIncreaseAmount)
    {
        if (_view == null)
        {
            return;
        }

        BindButton(_view.AddFuelTabButton, onAddFuelTab);
        BindButton(_view.CookingTabButton, onCookingTab);
        BindButton(_view.WaterTabButton, onWaterTab);
        BindButton(_view.ActionButton, onAction);
        BindButton(_view.CloseButton, onClose);

        _onDecreaseAmount = onDecreaseAmount;
        _onIncreaseAmount = onIncreaseAmount;
    }

    public void Show()
    {
        if (_view != null && _view.Root != null)
        {
            _view.Root.SetActive(true);
        }
    }

    public void Hide()
    {
        if (_view != null && _view.Root != null)
        {
            _view.Root.SetActive(false);
        }
    }

    public void RebuildList(IReadOnlyList<FireBurningOperationListEntry> entries, int selectedIndex, Action<int> onSelected, string amountText, bool canDecreaseAmount, bool canIncreaseAmount)
    {
        ClearList();

        if (_view == null || _view.ListRoot == null || _view.ListItemPrefab == null || entries == null)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            int capturedIndex = i;
            FireOperationListItemView itemView = UnityEngine.Object.Instantiate(_view.ListItemPrefab, _view.ListRoot);
            itemView.Refresh(entries[i], i == selectedIndex, () => onSelected?.Invoke(capturedIndex), amountText, canDecreaseAmount, canIncreaseAmount, _onDecreaseAmount, _onIncreaseAmount);
            _spawnedItems.Add(itemView);
        }
    }

    public void SetBurningTime(string text)
    {
        if (_view == null)
        {
            return;
        }

        if (_view.BurningTimeText != null)
        {
            _view.BurningTimeText.text = text;
        }
    }

    public void SetAction(string text, bool interactable)
    {
        if (_view == null)
        {
            return;
        }

        if (_view.ActionButtonText != null)
        {
            _view.ActionButtonText.text = text;
        }

        if (_view.ActionButton != null)
        {
            _view.ActionButton.interactable = interactable;
        }
    }

    public void ClearList()
    {
        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            if (_spawnedItems[i] != null)
            {
                UnityEngine.Object.Destroy(_spawnedItems[i].gameObject);
            }
        }

        _spawnedItems.Clear();
    }

    private static void BindButton(Button button, Action action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action?.Invoke());
    }
}
