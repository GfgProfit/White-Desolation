using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireStartWindowPresenter
{
    private readonly GameObject _root;
    private readonly FireChoiceView _igniterView;
    private readonly FireChoiceView _tinderView;
    private readonly FireChoiceView _fuelView;
    private readonly FireChoiceView _accelerantView;
    private readonly TMP_Text _baseChanceText;
    private readonly TMP_Text _successChanceText;
    private readonly TMP_Text _burnTimeText;
    private readonly Button _startButton;
    private readonly Button _closeButton;

    public bool IsOpen => _root != null && _root.activeSelf;

    public FireStartWindowPresenter(GameObject root, FireChoiceView igniterView, FireChoiceView tinderView, FireChoiceView fuelView, FireChoiceView accelerantView, TMP_Text baseChanceText, TMP_Text successChanceText, TMP_Text burnTimeText, Button startButton, Button closeButton)
    {
        _root = root;
        _igniterView = igniterView;
        _tinderView = tinderView;
        _fuelView = fuelView;
        _accelerantView = accelerantView;
        _baseChanceText = baseChanceText;
        _successChanceText = successChanceText;
        _burnTimeText = burnTimeText;
        _startButton = startButton;
        _closeButton = closeButton;
    }

    public void Bind(Action onPreviousIgniter, Action onNextIgniter, Action onPreviousTinder, Action onNextTinder, Action onPreviousFuel, Action onNextFuel, Action onPreviousAccelerant, Action onNextAccelerant, Action onStart, Action onClose)
    {
        _igniterView?.Bind(onPreviousIgniter, onNextIgniter);
        _tinderView?.Bind(onPreviousTinder, onNextTinder);
        _fuelView?.Bind(onPreviousFuel, onNextFuel);
        _accelerantView?.Bind(onPreviousAccelerant, onNextAccelerant);

        if (_startButton != null)
        {
            _startButton.onClick.RemoveAllListeners();
            _startButton.onClick.AddListener(() => onStart?.Invoke());
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() => onClose?.Invoke());
        }
    }

    public void Show()
    {
        if (_root != null)
        {
            _root.SetActive(true);
        }
    }

    public void Hide()
    {
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    public void Refresh(FireStartPlan plan, FireStartingConfig config, InventoryController inventory, float accelerantAmountCost)
    {
        if (plan == null)
        {
            return;
        }

        _igniterView?.Refresh(plan.Igniter, BuildItemAmountText(inventory, plan.Igniter, 1f));
        _tinderView?.Refresh(plan.Tinder, BuildItemAmountText(inventory, plan.Tinder, 1f));
        _fuelView?.Refresh(plan.Fuel, BuildItemAmountText(inventory, plan.Fuel, 1f));
        _accelerantView?.Refresh(plan.Accelerant, BuildItemAmountText(inventory, plan.Accelerant, accelerantAmountCost));

        if (_baseChanceText != null)
        {
            _baseChanceText.text = config != null ? $"{config.BaseChance:0}%" : string.Empty;
        }

        if (_successChanceText != null)
        {
            _successChanceText.text = $"{plan.SuccessChance:0}%";
        }

        if (_burnTimeText != null)
        {
            _burnTimeText.text = $"{FormatMinutes(plan.BurnMinutes)}";
        }

        if (_startButton != null)
        {
            bool canStart = plan.HasRequiredItems && FireStartCostValidator.CanPay(inventory, plan.AttemptCost) && FireStartCostValidator.CanPay(inventory, plan.SuccessCost);
            _startButton.interactable = canStart;
        }
    }

    private static string BuildItemAmountText(InventoryController inventory, ItemData itemData, float requiredAmount)
    {
        if (itemData == null || inventory == null)
        {
            return string.Empty;
        }

        if (itemData.UsesCustomAmount)
        {
            float currentAmount = inventory.GetTotalAmount(itemData);
            return $"{FormatAmount(requiredAmount)} л / {FormatAmount(currentAmount)} л";
        }

        int currentCount = inventory.GetTotalCount(itemData);
        return $"1 из {currentCount}";
    }

    private static string FormatAmount(float amount)
    {
        return amount.ToString("0.##");
    }

    private static string FormatMinutes(float minutes)
    {
        int totalMinutes = Mathf.CeilToInt(Mathf.Max(0f, minutes));
        int hours = totalMinutes / 60;
        int restMinutes = totalMinutes % 60;

        if (hours > 0)
        {
            return $"{hours} ч {restMinutes:00} мин";
        }

        return $"{restMinutes} мин";
    }
}