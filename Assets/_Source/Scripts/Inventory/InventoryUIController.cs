using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    private const string DebugPrefix = "[InventoryUI]";
    private const float ZeroTolerance = 0.0001f;

    [Header("Root")]
    [SerializeField] private GameObject _inventoryRoot;

    [Header("Grid")]
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private InventoryItemCellView _cellPrefab;

    [Header("Weight Display")]
    [SerializeField] private TMP_Text _carryWeightText;
    [SerializeField] private TMP_Text _currentWeightText;
    [SerializeField] private Slider _carryWeightSlider;

    [Header("Details")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemDescriptionText;
    [SerializeField] private TMP_Text _itemCountText;

    [Header("Consumable Use Thresholds")]
    [SerializeField, Min(0f)] private float _maxThirstToAllowConsumableUse = 0.7f;
    [SerializeField, Min(0f)] private float _maxHungerToAllowConsumableUse = 2300f;

    [Header("Stats")]
    [SerializeField] private GameObject _durabilityHolder;
    [SerializeField] private TMP_Text _durabilityText;

    [SerializeField] private GameObject _weightHolder;
    [SerializeField] private TMP_Text _weightText;

    [SerializeField] private GameObject _caloriesHolder;
    [SerializeField] private TMP_Text _caloriesText;

    [SerializeField] private GameObject _hydrationHolder;
    [SerializeField] private TMP_Text _hydrationText;

    [Header("Buttons")]
    [SerializeField] private Button _useButton;
    [SerializeField] private TMP_Text _useButtonLabel;
    [SerializeField] private Button _dropOneButton;

    [Header("Use Progress Modal")]
    [SerializeField] private GameObject _useProgressModalRoot;
    [SerializeField] private Image _useProgressFillImage;
    [SerializeField] private TMP_Text _useProgressText;
    [SerializeField, Min(0.01f)] private float _useDurationSeconds = 5f;

    [Header("Scene References")]
    [SerializeField] private PlayerNeedsController _playerNeedsController;

    [Header("Optional")]
    [SerializeField] private Behaviour[] _disableWhileOpen;

    [Inject] private InventoryController _inventoryController;
    [Inject] private IPlayerInput _playerInput;

    private readonly List<InventoryItemCellView> _spawnedCells = new();

    private bool _isOpen;
    private bool _isUsingItem;
    private int _selectedIndex = -1;
    private Coroutine _useRoutine;

    private struct UsePlan
    {
        public int SlotIndex;
        public ItemPrimaryActionType ActionType;
        public string VerbText;
        public float Duration;
        public float HydrationToApply;
        public float CaloriesToApply;
        public float HydrationStateToConsume;
        public float CaloriesStateToConsume;
        public float AmountToConsume;

        public bool HasInventoryConsume =>
            !Mathf.Approximately(HydrationStateToConsume, 0f) ||
            !Mathf.Approximately(CaloriesStateToConsume, 0f) ||
            !Mathf.Approximately(AmountToConsume, 0f);

        public bool HasPlayerEffect =>
            !Mathf.Approximately(HydrationToApply, 0f) ||
            !Mathf.Approximately(CaloriesToApply, 0f);
    }

    private void Awake()
    {
        if (_inventoryRoot != null)
            _inventoryRoot.SetActive(false);

        SetUseProgressVisible(false);
        SetUseProgress(0f, string.Empty);

        if (_useButton != null)
            _useButton.onClick.AddListener(HandleUseClicked);

        if (_dropOneButton != null)
            _dropOneButton.onClick.AddListener(HandleDropOneClicked);
    }

    private void Start()
    {
        if (_inventoryController == null)
        {
            Debug.LogError($"{DebugPrefix} InventoryController is null.");
            return;
        }

        _inventoryController.OnInventoryChanged += RefreshView;
        RefreshView();
    }

    private void OnDestroy()
    {
        if (_inventoryController != null)
            _inventoryController.OnInventoryChanged -= RefreshView;

        if (_useButton != null)
            _useButton.onClick.RemoveListener(HandleUseClicked);

        if (_dropOneButton != null)
            _dropOneButton.onClick.RemoveListener(HandleDropOneClicked);

        if (_useRoutine != null)
            StopCoroutine(_useRoutine);
    }

    private void Update()
    {
        if (_playerInput == null)
            return;

        if (_isUsingItem)
            return;

        if (!_isOpen && _playerInput.IsInventoryPressed())
        {
            Open();
            return;
        }

        if (_isOpen && (_playerInput.IsInventoryPressed() || _playerInput.IsEscapePressed()))
        {
            Close();
        }
    }

    private void Open()
    {
        _isOpen = true;

        if (_inventoryRoot != null)
            _inventoryRoot.SetActive(true);

        SetBlockedBehaviours(false);
        SetCursorState(true);
        RefreshView();
    }

    private void Close()
    {
        if (_isUsingItem)
            return;

        _isOpen = false;

        if (_inventoryRoot != null)
            _inventoryRoot.SetActive(false);

        SetBlockedBehaviours(true);
        SetCursorState(false);
    }

    private void RefreshView()
    {
        if (_inventoryController == null)
            return;

        RefreshCarryWeight();

        if (_inventoryController.SlotCount == 0)
        {
            _selectedIndex = -1;
        }
        else
        {
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _inventoryController.SlotCount - 1);

            if (_selectedIndex < 0)
                _selectedIndex = 0;
        }

        RebuildGrid();
        RefreshDetails();
    }

    private void RefreshCarryWeight()
    {
        if (_carryWeightText == null || _inventoryController == null)
            return;

        _carryWeightText.text = InventoryDisplayFormatter.FormatCarryWeight(
            _inventoryController.CurrentCarryWeightKg,
            _inventoryController.MaxCarryWeightKg);

        if (_currentWeightText != null)
        {
            _currentWeightText.text = InventoryDisplayFormatter.FormatCarryWeight(
                _inventoryController.CurrentCarryWeightKg,
                0f);
        }

        if (_carryWeightSlider != null)
        {
            _carryWeightSlider.maxValue = _inventoryController.MaxCarryWeightKg;
            _carryWeightSlider.value = _inventoryController.CurrentCarryWeightKg;
        }
    }

    private void RebuildGrid()
    {
        for (int i = 0; i < _spawnedCells.Count; i++)
        {
            if (_spawnedCells[i] != null)
                Destroy(_spawnedCells[i].gameObject);
        }

        _spawnedCells.Clear();

        if (_inventoryController == null || _cellPrefab == null || _gridRoot == null)
            return;

        for (int i = 0; i < _inventoryController.SlotCount; i++)
        {
            InventorySlot slot = _inventoryController.GetSlotAt(i);
            InventoryItemCellView cell = Instantiate(_cellPrefab, _gridRoot);
            cell.Bind(slot, i, i == _selectedIndex, HandleSlotSelected);
            _spawnedCells.Add(cell);
        }
    }

    private void HandleSlotSelected(int slotIndex)
    {
        if (_isUsingItem)
            return;

        _selectedIndex = slotIndex;

        for (int i = 0; i < _spawnedCells.Count; i++)
        {
            _spawnedCells[i].SetSelected(i == _selectedIndex);
        }

        RefreshDetails();
    }

    private void RefreshDetails()
    {
        InventorySlot slot = _inventoryController != null
            ? _inventoryController.GetSlotAt(_selectedIndex)
            : null;

        bool hasSelection = slot != null && !slot.IsEmpty && slot.Item != null;
        bool canUseSelected = hasSelection && CanUseSlot(slot);
        bool canDrop = hasSelection && !_isUsingItem;

        if (_useButton != null)
            _useButton.interactable = canUseSelected;

        if (_dropOneButton != null)
            _dropOneButton.interactable = canDrop;

        if (_useButtonLabel != null)
            _useButtonLabel.text = hasSelection
                ? InventoryDisplayFormatter.FormatPrimaryActionLabel(slot)
                : "Использовать";

        if (!hasSelection)
        {
            if (_itemIcon != null)
            {
                _itemIcon.enabled = false;
                _itemIcon.sprite = null;
            }

            if (_itemNameText != null)
                _itemNameText.text = "Не выбран предмет.";

            if (_itemDescriptionText != null)
                _itemDescriptionText.text = string.Empty;

            if (_itemCountText != null)
                _itemCountText.text = string.Empty;

            ClearStats();
            return;
        }

        if (_itemIcon != null)
        {
            _itemIcon.enabled = slot.Item.Icon != null;
            _itemIcon.sprite = slot.Item.Icon;
        }

        if (_itemNameText != null)
            _itemNameText.text = slot.Item.DisplayName;

        if (_itemDescriptionText != null)
            _itemDescriptionText.text = slot.Item.Description;

        if (_itemCountText != null)
            _itemCountText.text = InventoryDisplayFormatter.FormatPrimaryValue(slot);

        RefreshStats(slot);
    }

    private void RefreshStats(InventorySlot slot)
    {
        SetStatRow(
            _durabilityHolder,
            _durabilityText,
            InventoryDisplayFormatter.TryGetDurabilityText(slot, out string durabilityText),
            durabilityText);

        SetStatRow(
            _weightHolder,
            _weightText,
            InventoryDisplayFormatter.TryGetWeightText(slot, out string weightText),
            weightText);

        SetStatRow(
            _caloriesHolder,
            _caloriesText,
            InventoryDisplayFormatter.TryGetCaloriesText(slot, out string caloriesText),
            caloriesText);

        SetStatRow(
            _hydrationHolder,
            _hydrationText,
            InventoryDisplayFormatter.TryGetHydrationText(slot, out string hydrationText),
            hydrationText);
    }

    private void ClearStats()
    {
        SetStatRow(_durabilityHolder, _durabilityText, false, string.Empty);
        SetStatRow(_weightHolder, _weightText, false, string.Empty);
        SetStatRow(_caloriesHolder, _caloriesText, false, string.Empty);
        SetStatRow(_hydrationHolder, _hydrationText, false, string.Empty);
    }

    private void SetStatRow(GameObject holder, TMP_Text textComponent, bool visible, string value)
    {
        if (holder != null)
            holder.SetActive(visible);

        if (textComponent != null)
            textComponent.text = visible ? value : string.Empty;
    }

    private void HandleUseClicked()
    {
        if (_inventoryController == null || _isUsingItem)
            return;

        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);
        if (slot == null || slot.Item == null)
            return;

        if (!CanUseSlot(slot))
        {
            LogUseBlockedReason(slot);
            return;
        }

        if (!TryBuildUsePlan(_selectedIndex, slot, out UsePlan plan))
        {
            Debug.Log($"{DebugPrefix} {slot.Item.DisplayName} cannot be used right now.");
            return;
        }

        _useRoutine = StartCoroutine(ExecuteUseRoutine(plan));
    }

    private IEnumerator ExecuteUseRoutine(UsePlan plan)
    {
        _isUsingItem = true;
        RefreshDetails();

        SetUseProgressVisible(true);
        SetUseProgress(0f, plan.VerbText);

        float elapsed = 0f;
        float appliedHydration = 0f;
        float appliedCalories = 0f;

        while (elapsed < plan.Duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / plan.Duration);
            SetUseProgress(progress, plan.VerbText);

            float targetHydration = plan.HydrationToApply * progress;
            float targetCalories = plan.CaloriesToApply * progress;

            float hydrationDelta = targetHydration - appliedHydration;
            float caloriesDelta = targetCalories - appliedCalories;

            if (Mathf.Abs(hydrationDelta) > ZeroTolerance)
            {
                float actualHydrationDelta = ApplyHydrationDelta(hydrationDelta);
                appliedHydration += actualHydrationDelta;
            }

            if (Mathf.Abs(caloriesDelta) > ZeroTolerance)
            {
                float actualCaloriesDelta = ApplyCaloriesDelta(caloriesDelta);
                appliedCalories += actualCaloriesDelta;
            }

            yield return null;
        }

        SetUseProgress(1f, plan.VerbText);

        if (plan.HasInventoryConsume)
        {
            _inventoryController.TryConsumeFromSlot(
                plan.SlotIndex,
                plan.HydrationStateToConsume,
                plan.CaloriesStateToConsume,
                plan.AmountToConsume);
        }

        if (plan.ActionType == ItemPrimaryActionType.Action)
        {
            InventorySlot slot = _inventoryController.GetSlotAt(plan.SlotIndex);
            string itemName = slot != null && slot.Item != null ? slot.Item.DisplayName : "item";
            Debug.Log($"{DebugPrefix} Action completed for {itemName}.");
        }

        SetUseProgressVisible(false);
        SetUseProgress(0f, string.Empty);

        _isUsingItem = false;
        _useRoutine = null;

        RefreshView();
    }

    private float ApplyHydrationDelta(float hydrationDelta)
    {
        if (_playerNeedsController == null)
            return 0f;

        if (Mathf.Abs(hydrationDelta) <= ZeroTolerance)
            return 0f;

        if (hydrationDelta > 0f)
            return _playerNeedsController.RestoreThirstUpTo(hydrationDelta);

        float before = _playerNeedsController.Thirst;
        _playerNeedsController.AddThirst(hydrationDelta);
        return _playerNeedsController.Thirst - before;
    }

    private float ApplyCaloriesDelta(float caloriesDelta)
    {
        if (_playerNeedsController == null)
            return 0f;

        if (Mathf.Abs(caloriesDelta) <= ZeroTolerance)
            return 0f;

        if (caloriesDelta > 0f)
            return _playerNeedsController.RestoreHungerUpTo(caloriesDelta);

        float before = _playerNeedsController.Hunger;
        _playerNeedsController.AddHunger(caloriesDelta);
        return _playerNeedsController.Hunger - before;
    }

    private float RestoreHydrationDelta(float hydrationDelta)
    {
        if (_playerNeedsController == null)
            return 0f;

        return _playerNeedsController.RestoreThirstUpTo(hydrationDelta);
    }

    private float RestoreCaloriesDelta(float caloriesDelta)
    {
        if (_playerNeedsController == null)
            return 0f;

        return _playerNeedsController.RestoreHungerUpTo(caloriesDelta);
    }

    private bool TryBuildUsePlan(int slotIndex, InventorySlot slot, out UsePlan plan)
    {
        plan = default;

        if (slot == null || slot.Item == null)
            return false;

        plan.SlotIndex = slotIndex;
        plan.ActionType = slot.Item.PrimaryAction;
        plan.VerbText = ResolveUseVerb(slot);
        plan.Duration = _useDurationSeconds;

        switch (slot.Item.PrimaryAction)
        {
            case ItemPrimaryActionType.Use:
                return TryBuildConsumableUsePlan(slot, ref plan);

            case ItemPrimaryActionType.Action:
                return true;

            default:
                return false;
        }
    }

    private bool TryBuildConsumableUsePlan(InventorySlot slot, ref UsePlan plan)
    {
        if (_playerNeedsController == null)
        {
            Debug.LogWarning($"{DebugPrefix} PlayerNeedsController is null.");
            return false;
        }

        if (!PassesConsumableUseThresholds(slot))
            return false;

        // Чистая вода по объёму
        if (IsVolumeDrink(slot))
        {
            float hydrationToApply = Mathf.Min(slot.CurrentAmount, _playerNeedsController.MissingThirst);
            if (hydrationToApply <= ZeroTolerance)
                return false;

            plan.HydrationToApply = hydrationToApply;
            plan.AmountToConsume = hydrationToApply;
            return true;
        }

        float useRatio = CalculateConsumableUseRatio(slot);
        if (useRatio <= ZeroTolerance)
            return false;

        if (Mathf.Abs(slot.CurrentHydration) > ZeroTolerance)
        {
            float hydrationAmount = slot.CurrentHydration * useRatio;
            plan.HydrationToApply = hydrationAmount;
            plan.HydrationStateToConsume = hydrationAmount;
        }

        if (Mathf.Abs(slot.CurrentCalories) > ZeroTolerance)
        {
            float caloriesAmount = slot.CurrentCalories * useRatio;
            plan.CaloriesToApply = caloriesAmount;
            plan.CaloriesStateToConsume = caloriesAmount;
        }

        if (slot.HasAmount && slot.CurrentAmount > ZeroTolerance)
        {
            plan.AmountToConsume = slot.CurrentAmount * useRatio;
        }

        return plan.HasPlayerEffect || plan.HasInventoryConsume;
    }

    private float CalculateConsumableUseRatio(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || _playerNeedsController == null)
            return 0f;

        float ratio = 1f;
        bool hasPositiveEffect = false;
        bool hasAnyEffect = false;

        if (Mathf.Abs(slot.CurrentHydration) > ZeroTolerance)
        {
            hasAnyEffect = true;

            if (slot.CurrentHydration > ZeroTolerance)
            {
                hasPositiveEffect = true;
                ratio = Mathf.Min(ratio, _playerNeedsController.MissingThirst / slot.CurrentHydration);
            }
        }

        if (Mathf.Abs(slot.CurrentCalories) > ZeroTolerance)
        {
            hasAnyEffect = true;

            if (slot.CurrentCalories > 0f)
            {
                hasPositiveEffect = true;
                ratio = Mathf.Min(ratio, _playerNeedsController.MissingHunger / slot.CurrentCalories);
            }
        }

        if (!hasAnyEffect)
            return 0f;

        if (!hasPositiveEffect)
            return 1f;

        return Mathf.Clamp01(ratio);
    }

    private string ResolveUseVerb(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return "использует";

        if (slot.Item.Category == ItemCategory.Water)
            return "пьет";

        if (slot.Item.Category == ItemCategory.Food)
            return "ест";

        if (slot.Item.Category == ItemCategory.Resource)
            return "собирает";

        if (slot.Item.Category == ItemCategory.Tool)
            return "ремонтирует";

        return "открывает";
    }

    private static bool IsVolumeDrink(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return false;

        if (slot.Item.PrimaryAction != ItemPrimaryActionType.Use)
            return false;

        if (slot.Item.Category != ItemCategory.Water)
            return false;

        if (!slot.HasAmount)
            return false;

        if (slot.Item.AmountUnit != ItemAmountUnit.Liter)
            return false;

        if (slot.CurrentAmount <= ZeroTolerance)
            return false;

        // Ветка "чистая вода" должна работать только для предметов,
        // у которых нет калорий вообще.
        if (slot.Item.RestoreCalories > 0)
            return false;

        if (slot.CurrentCalories > ZeroTolerance)
            return false;

        return true;
    }

    private void SetUseProgressVisible(bool visible)
    {
        if (_useProgressModalRoot != null)
            _useProgressModalRoot.SetActive(visible);
    }

    private void SetUseProgress(float progress01, string text)
    {
        if (_useProgressFillImage != null)
            _useProgressFillImage.fillAmount = Mathf.Clamp01(progress01);

        if (_useProgressText != null)
            _useProgressText.text = text;
    }

    private bool CanUseSlot(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return false;

        if (_isUsingItem)
            return false;

        return slot.Item.PrimaryAction switch
        {
            ItemPrimaryActionType.Use => CanUseConsumableSlot(slot),
            ItemPrimaryActionType.Action => true,
            _ => false
        };
    }

    private bool CanUseConsumableSlot(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || _playerNeedsController == null)
            return false;

        if (!PassesConsumableUseThresholds(slot))
            return false;

        if (IsVolumeDrink(slot))
            return slot.CurrentAmount > ZeroTolerance && _playerNeedsController.MissingThirst > ZeroTolerance;

        bool hasHydrationEffect = Mathf.Abs(slot.CurrentHydration) > ZeroTolerance;
        bool hasCaloriesEffect = Mathf.Abs(slot.CurrentCalories) > ZeroTolerance;

        return hasHydrationEffect || hasCaloriesEffect;
    }

    private bool PassesConsumableUseThresholds(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || _playerNeedsController == null)
            return false;

        bool affectsHydration = DoesAffectHydration(slot);
        bool affectsCalories = DoesAffectCalories(slot);

        if (affectsHydration && _playerNeedsController.Thirst > _maxThirstToAllowConsumableUse)
            return false;

        if (affectsCalories && _playerNeedsController.Hunger > _maxHungerToAllowConsumableUse)
            return false;

        return true;
    }

    private bool DoesAffectHydration(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return false;

        if (IsVolumeDrink(slot))
            return true;

        return Mathf.Abs(slot.CurrentHydration) > ZeroTolerance;
    }

    private bool DoesAffectCalories(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return false;

        return Mathf.Abs(slot.CurrentCalories) > ZeroTolerance;
    }

    private void LogUseBlockedReason(InventorySlot slot)
    {
        if (slot == null || slot.Item == null || _playerNeedsController == null)
            return;

        bool affectsHydration = DoesAffectHydration(slot);
        bool affectsCalories = DoesAffectCalories(slot);

        bool thirstBlocked = affectsHydration && _playerNeedsController.Thirst > _maxThirstToAllowConsumableUse;
        bool hungerBlocked = affectsCalories && _playerNeedsController.Hunger > _maxHungerToAllowConsumableUse;

        if (thirstBlocked && hungerBlocked)
        {
            Debug.Log(
                $"{DebugPrefix} {slot.Item.DisplayName} blocked by thresholds. " +
                $"Thirst={_playerNeedsController.Thirst:0.##}>{_maxThirstToAllowConsumableUse:0.##}, " +
                $"Hunger={_playerNeedsController.Hunger:0.##}>{_maxHungerToAllowConsumableUse:0.##}");
            return;
        }

        if (thirstBlocked)
        {
            Debug.Log(
                $"{DebugPrefix} {slot.Item.DisplayName} blocked by thirst threshold. " +
                $"Thirst={_playerNeedsController.Thirst:0.##}>{_maxThirstToAllowConsumableUse:0.##}");
            return;
        }

        if (hungerBlocked)
        {
            Debug.Log(
                $"{DebugPrefix} {slot.Item.DisplayName} blocked by hunger threshold. " +
                $"Hunger={_playerNeedsController.Hunger:0.##}>{_maxHungerToAllowConsumableUse:0.##}");
        }
    }

    private void HandleDropOneClicked()
    {
        if (_isUsingItem)
            return;

        InventorySlot slot = _inventoryController.GetSlotAt(_selectedIndex);
        if (slot == null || slot.Item == null)
            return;

        _inventoryController.TryRemoveFromSlot(_selectedIndex, 1);
    }

    private void SetBlockedBehaviours(bool enabled)
    {
        if (_disableWhileOpen == null)
            return;

        for (int i = 0; i < _disableWhileOpen.Length; i++)
        {
            if (_disableWhileOpen[i] != null)
                _disableWhileOpen[i].enabled = enabled;
        }
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}