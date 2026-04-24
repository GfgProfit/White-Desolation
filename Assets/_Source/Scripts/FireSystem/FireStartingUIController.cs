using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireStartingUIController : MonoBehaviour
{
    private const float AccelerantAmountCost = 0.3f;
    private const float FlintDurabilityCost = 2f;

    [Header("Data")]
    [SerializeField] private FireStartingConfig _config;

    [Header("Start Fire Window")]
    [SerializeField] private GameObject _startRoot;
    [SerializeField] private FireStartingChoiceView _igniterView;
    [SerializeField] private FireStartingChoiceView _tinderView;
    [SerializeField] private FireStartingChoiceView _fuelView;
    [SerializeField] private FireStartingChoiceView _accelerantView;
    [SerializeField] private TMP_Text _baseChanceText;
    [SerializeField] private TMP_Text _successChanceText;
    [SerializeField] private TMP_Text _burnTimeText;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _closeButton;

    [Header("Progress Window")]
    [SerializeField] private FireProgressView _progressView;
    [SerializeField, Min(0.1f)] private float _defaultStartDurationSeconds = 5f;
    [SerializeField, Min(0.1f)] private float _accelerantStartDurationSeconds = 2f;
    [SerializeField, Range(0.05f, 0.95f)] private float _failedMinFill = 0.1f;
    [SerializeField, Range(0.05f, 0.95f)] private float _failedMaxFill = 0.85f;

    [Header("Burning Stub Window")]
    [SerializeField] private GameObject _burningStubRoot;
    [SerializeField] private TMP_Text _burningStubText;
    [SerializeField] private Button _burningStubCloseButton;

    [Header("Player Lock")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [SerializeField] private GameObject[] _objectsDisableWhileOpen;

    [Inject] private readonly InventoryController _inventory;

    private readonly List<ItemData> _availableIgniters = new();
    private readonly List<ItemData> _availableTinders = new();
    private readonly List<ItemData> _availableFuels = new();
    private readonly List<ItemData> _availableAccelerants = new();

    private int _igniterIndex;
    private int _tinderIndex;
    private int _fuelIndex;
    private int _accelerantIndex = -1;

    private FireSourceInteractable _currentSource;
    private Coroutine _startRoutine;

    private FireStartingSelection CurrentSelection => new()
    {
        Igniter = GetSelected(_availableIgniters, _igniterIndex),
        Tinder = GetSelected(_availableTinders, _tinderIndex),
        Fuel = GetSelected(_availableFuels, _fuelIndex),
        Accelerant = GetSelectedOptional(_availableAccelerants, _accelerantIndex)
    };

    private void Awake()
    {
        SetStartVisible(false);
        SetBurningStubVisible(false);
        _progressView?.Hide();

        _igniterView?.Bind(() => StepIndex(ref _igniterIndex, _availableIgniters.Count, -1), () => StepIndex(ref _igniterIndex, _availableIgniters.Count, 1));
        _tinderView?.Bind(() => StepIndex(ref _tinderIndex, _availableTinders.Count, -1), () => StepIndex(ref _tinderIndex, _availableTinders.Count, 1));
        _fuelView?.Bind(() => StepIndex(ref _fuelIndex, _availableFuels.Count, -1), () => StepIndex(ref _fuelIndex, _availableFuels.Count, 1));
        _accelerantView?.Bind(() => StepOptionalIndex(ref _accelerantIndex, _availableAccelerants.Count, -1), () => StepOptionalIndex(ref _accelerantIndex, _availableAccelerants.Count, 1));

        if (_startButton != null)
        {
            _startButton.onClick.RemoveAllListeners();
            _startButton.onClick.AddListener(StartFireAttempt);
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(CloseAll);
        }

        if (_burningStubCloseButton != null)
        {
            _burningStubCloseButton.onClick.RemoveAllListeners();
            _burningStubCloseButton.onClick.AddListener(CloseAll);
        }
    }

    private void Update()
    {
        if ((_startRoot != null && _startRoot.activeSelf) || (_burningStubRoot != null && _burningStubRoot.activeSelf))
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                CloseAll();
            }
        }
    }

    public void OpenFireStarting(FireSourceInteractable source)
    {
        if (source == null || _config == null || _inventory == null)
        {
            Debug.LogWarning("[FireStarting] Cannot open fire starting window: missing source/config/inventory.");
            return;
        }

        _currentSource = source;
        RebuildAvailableItems();
        ResetSelectionIndexes();
        RefreshAllViews();
        SetPlayerControlsEnabled(false);
        SetObjectsEnabled(false);
        SetBurningStubVisible(false);
        _progressView?.Hide();
        SetStartVisible(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OpenBurningStub(FireSourceInteractable source)
    {
        _currentSource = source;
        SetStartVisible(false);
        _progressView?.Hide();
        SetPlayerControlsEnabled(false);
        SetObjectsEnabled(false);

        if (_burningStubText != null && source != null)
        {
            _burningStubText.text = $"{source.DisplayName}\nОгонь уже разведён.\nВремя горения: {FormatMinutes(source.RemainingBurnMinutes)}\nТемпература: 0.0 C";
        }

        SetBurningStubVisible(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseAll()
    {
        if (_startRoutine != null)
        {
            StopCoroutine(_startRoutine);
            _startRoutine = null;
        }

        SetStartVisible(false);
        SetBurningStubVisible(false);
        _progressView?.Hide();
        SetPlayerControlsEnabled(true);
        SetObjectsEnabled(true);
        _currentSource = null;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void StartFireAttempt()
    {
        if (_startRoutine != null)
        {
            return;
        }

        FireStartingSelection selection = CurrentSelection;
        if (!selection.HasRequiredItems)
        {
            Debug.LogWarning("[FireStarting] Igniter, tinder and fuel are required.");
            RefreshAllViews();
            return;
        }

        if (!ConsumeIgniter(selection.Igniter))
        {
            Debug.LogWarning("[FireStarting] Failed to consume igniter.");
            RebuildAvailableItems();
            RefreshAllViews();
            return;
        }

        float successChance = CalculateSuccessChance(selection);
        bool success = selection.UsesAccelerant || Random.value <= successChance / 100f;
        float duration = selection.UsesAccelerant ? _accelerantStartDurationSeconds : _defaultStartDurationSeconds;
        float targetFill = success ? 1f : Random.Range(_failedMinFill, _failedMaxFill);

        SetStartVisible(false);
        _progressView?.Show("разводим огонь");
        _startRoutine = StartCoroutine(FireProgressRoutine(selection, success, targetFill, duration));
    }

    private IEnumerator FireProgressRoutine(FireStartingSelection selection, bool success, float targetFill, float duration)
    {
        float elapsed = 0f;
        float actualDuration = success ? duration : Mathf.Max(0.1f, duration * Mathf.Clamp01(targetFill));

        while (elapsed < actualDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / actualDuration);
            _progressView?.SetFill(Mathf.Lerp(0f, targetFill, t));
            yield return null;
        }

        _progressView?.SetFill(targetFill);

        if (success)
        {
            ConsumeSuccessItems(selection);
            FireFuelStats stats = _config.GetFuelStats(selection.Fuel);
            _currentSource?.Ignite(stats.BurnMinutes);
        }

        _startRoutine = null;
        CloseAll();
    }

    private void RebuildAvailableItems()
    {
        FillAvailable(_availableIgniters, _config.Igniters, false);
        FillAvailable(_availableTinders, _config.Tinders, false);
        FillAvailable(_availableFuels, _config.Fuels, false);
        FillAvailable(_availableAccelerants, _config.Accelerants, true);
    }

    private void FillAvailable(List<ItemData> result, ItemData[] source, bool isAccelerant)
    {
        result.Clear();

        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Length; i++)
        {
            ItemData item = source[i];
            if (item == null || Contains(result, item))
            {
                continue;
            }

            bool available = isAccelerant && item.UsesCustomAmount
                ? HasCustomAmount(item, AccelerantAmountCost)
                : _inventory.ContainsUsableItem(item, 1);

            if (available)
            {
                result.Add(item);
            }
        }
    }

    private void ResetSelectionIndexes()
    {
        _igniterIndex = _availableIgniters.Count > 0 ? 0 : -1;
        _tinderIndex = _availableTinders.Count > 0 ? 0 : -1;
        _fuelIndex = _availableFuels.Count > 0 ? 0 : -1;
        _accelerantIndex = -1;
    }

    private void RefreshAllViews()
    {
        FireStartingSelection selection = CurrentSelection;
        FireFuelStats fuelStats = _config != null ? _config.GetFuelStats(selection.Fuel) : new FireFuelStats(0f, 0f);
        float successChance = CalculateSuccessChance(selection);

        _igniterView?.Refresh(selection.Igniter);
        _tinderView?.Refresh(selection.Tinder);
        _fuelView?.Refresh(selection.Fuel);
        _accelerantView?.Refresh(selection.Accelerant);

        if (_baseChanceText != null)
        {
            _baseChanceText.text = $"{_config.BaseStartChance:0}%";
        }

        if (_successChanceText != null)
        {
            _successChanceText.text = $"{successChance:0}%";
        }

        if (_burnTimeText != null)
        {
            _burnTimeText.text = $"{FormatMinutes(fuelStats.BurnMinutes)}";
        }

        if (_startButton != null)
        {
            _startButton.interactable = selection.HasRequiredItems;
        }
    }

    private float CalculateSuccessChance(FireStartingSelection selection)
    {
        if (_config == null)
        {
            return 0f;
        }

        if (selection != null && selection.UsesAccelerant)
        {
            return 100f;
        }

        FireFuelStats stats = _config.GetFuelStats(selection?.Fuel);
        return Mathf.Clamp(_config.BaseStartChance + stats.StartChanceBonus, 0f, 100f);
    }

    private bool ConsumeIgniter(ItemData igniter)
    {
        if (igniter == null || _inventory == null)
        {
            return false;
        }

        if (igniter.UsesDurability && !igniter.IsUnbreakable)
        {
            return _inventory.TryConsumeDurabilityFromFirstMatchingItem(igniter, FlintDurabilityCost);
        }

        return _inventory.TryRemoveItem(igniter, 1);
    }

    private void ConsumeSuccessItems(FireStartingSelection selection)
    {
        if (_inventory == null || selection == null)
        {
            return;
        }

        if (selection.Tinder != null)
        {
            _inventory.TryRemoveItem(selection.Tinder, 1);
        }

        if (selection.Fuel != null)
        {
            _inventory.TryRemoveItem(selection.Fuel, 1);
        }

        if (selection.Accelerant == null)
        {
            return;
        }

        if (selection.Accelerant.UsesCustomAmount)
        {
            TryConsumeCustomAmount(selection.Accelerant, AccelerantAmountCost);
        }
        else
        {
            _inventory.TryRemoveItem(selection.Accelerant, 1);
        }
    }

    private bool HasCustomAmount(ItemData item, float requiredAmount)
    {
        if (_inventory == null || item == null)
        {
            return false;
        }

        for (int i = 0; i < _inventory.Items.Count; i++)
        {
            InventorySlot slot = _inventory.Items[i];
            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                continue;
            }

            if (IsSameItem(slot.Item, item) && slot.HasAmount && slot.CurrentAmount >= requiredAmount)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryConsumeCustomAmount(ItemData item, float amount)
    {
        if (_inventory == null || item == null)
        {
            return false;
        }

        for (int i = 0; i < _inventory.Items.Count; i++)
        {
            InventorySlot slot = _inventory.Items[i];
            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                continue;
            }

            if (IsSameItem(slot.Item, item) && slot.HasAmount && slot.CurrentAmount >= amount)
            {
                return _inventory.TryConsumeFromSlot(i, amountToConsume: amount);
            }
        }

        return false;
    }

    private void StepIndex(ref int index, int count, int direction)
    {
        if (count <= 0)
        {
            index = -1;
            RefreshAllViews();
            return;
        }

        index = Mod(index + direction, count);
        RefreshAllViews();
    }

    private void StepOptionalIndex(ref int index, int count, int direction)
    {
        int totalStates = count + 1;
        int state = index + 1;
        state = Mod(state + direction, totalStates);
        index = state - 1;
        RefreshAllViews();
    }

    private void SetStartVisible(bool visible)
    {
        if (_startRoot != null)
        {
            _startRoot.SetActive(visible);
        }
    }

    private void SetBurningStubVisible(bool visible)
    {
        if (_burningStubRoot != null)
        {
            _burningStubRoot.SetActive(visible);
        }
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        if (_disableWhileOpen == null)
        {
            return;
        }

        for (int i = 0; i < _disableWhileOpen.Length; i++)
        {
            if (_disableWhileOpen[i] != null)
            {
                _disableWhileOpen[i].enabled = enabled;
            }
        }
    }

    private void SetObjectsEnabled(bool enabled)
    {
        if (_objectsDisableWhileOpen == null)
        {
            return;
        }

        for (int i = 0; i < _objectsDisableWhileOpen.Length; i++)
        {
            if (_objectsDisableWhileOpen[i] != null)
            {
                _objectsDisableWhileOpen[i].SetActive(enabled);
            }
        }
    }

    private static ItemData GetSelected(List<ItemData> items, int index)
    {
        if (items == null || index < 0 || index >= items.Count)
        {
            return null;
        }

        return items[index];
    }

    private static ItemData GetSelectedOptional(List<ItemData> items, int index)
    {
        return index < 0 ? null : GetSelected(items, index);
    }

    private static bool Contains(List<ItemData> items, ItemData item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (IsSameItem(items[i], item))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSameItem(ItemData a, ItemData b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(a.Id) && !string.IsNullOrWhiteSpace(b.Id))
        {
            return a.Id == b.Id;
        }

        return ReferenceEquals(a, b);
    }

    private static int Mod(int value, int divisor)
    {
        if (divisor <= 0)
        {
            return 0;
        }

        int result = value % divisor;
        return result < 0 ? result + divisor : result;
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
