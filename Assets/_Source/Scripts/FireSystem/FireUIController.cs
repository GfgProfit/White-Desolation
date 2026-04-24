using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireUIController : MonoBehaviour
{
    private const float AccelerantAmountCost = 0.3f;
    private const float FlintDurabilityCost01 = 0.02f;

    [Header("Data")]
    [SerializeField] private FireStartingConfig _config;

    [Header("Start Fire Window")]
    [SerializeField] private GameObject _startRoot;
    [SerializeField] private FireChoiceView _igniterView;
    [SerializeField] private FireChoiceView _tinderView;
    [SerializeField] private FireChoiceView _fuelView;
    [SerializeField] private FireChoiceView _accelerantView;
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

    [Inject] private InventoryController _inventory;

    private readonly List<ItemData> _availableIgniters = new List<ItemData>();
    private readonly List<ItemData> _availableTinders = new List<ItemData>();
    private readonly List<ItemData> _availableFuels = new List<ItemData>();
    private readonly List<ItemData> _availableAccelerants = new List<ItemData>();

    private int _igniterIndex = -1;
    private int _tinderIndex = -1;
    private int _fuelIndex = -1;
    private int _accelerantIndex = -1;

    private FireSourceInteractable _currentSource;
    private Coroutine _startRoutine;

    private void Awake()
    {
        SetStartVisible(false);
        SetBurningStubVisible(false);
        _progressView?.Hide();

        _igniterView?.Bind(PreviousIgniter, NextIgniter);
        _tinderView?.Bind(PreviousTinder, NextTinder);
        _fuelView?.Bind(PreviousFuel, NextFuel);
        _accelerantView?.Bind(PreviousAccelerant, NextAccelerant);

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
        bool startWindowOpen = _startRoot != null && _startRoot.activeSelf;
        bool burningStubOpen = _burningStubRoot != null && _burningStubRoot.activeSelf;

        if (!startWindowOpen && !burningStubOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse1))
        {
            CloseAll();
        }
    }

    public void OpenFireStarting(FireSourceInteractable source)
    {
        if (source == null)
        {
            return;
        }

        if (source.IsBurning)
        {
            OpenBurningStub(source);
            return;
        }

        if (_config == null)
        {
            Debug.LogWarning("[FireStarting] FireItemsConfig is missing.");
            return;
        }

        if (_inventory == null)
        {
            Debug.LogWarning("[FireStarting] InventoryController is missing.");
            return;
        }

        _currentSource = source;

        RebuildAvailableItems();
        ResetSelectionIndexes();
        RefreshAllViews();

        SetBurningStubVisible(false);
        _progressView?.Hide();

        SetPlayerControlsEnabled(false);
        SetObjectsEnabled(false);
        SetStartVisible(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OpenBurningStub(FireSourceInteractable source)
    {
        if (source == null)
        {
            return;
        }

        _currentSource = source;

        SetStartVisible(false);
        _progressView?.Hide();

        if (_burningStubText != null)
        {
            _burningStubText.text =
                $"{source.DisplayName}\n" +
                $"Огонь уже разведён.\n" +
                $"Время горения: {FormatMinutes(source.RemainingBurnMinutes)}\n" +
                $"Температура: 0.0 C";
        }

        SetPlayerControlsEnabled(false);
        SetObjectsEnabled(false);
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

        FireAttemptSnapshot snapshot = BuildCurrentSnapshot();

        if (!snapshot.HasRequiredItems)
        {
            Debug.LogWarning("[FireStarting] Нужны воспламенитель, трут и топливо.");
            RefreshAllViews();
            return;
        }

        if (!ConsumeIgniter(snapshot.Igniter))
        {
            Debug.LogWarning("[FireStarting] Не удалось потратить воспламенитель.");
            RebuildAvailableItems();
            ResetSelectionIndexes();
            RefreshAllViews();
            return;
        }

        bool success = snapshot.UsesAccelerant || Random.value <= snapshot.SuccessChance / 100f;

        float maxFailedFill = Mathf.Max(_failedMinFill, _failedMaxFill);
        float targetFill = success ? 1f : Random.Range(_failedMinFill, maxFailedFill);

        SetStartVisible(false);
        _progressView?.Show("разводим огонь");

        _startRoutine = StartCoroutine(FireProgressRoutine(snapshot, success, targetFill));
    }

    private IEnumerator FireProgressRoutine(FireAttemptSnapshot snapshot, bool success, float targetFill)
    {
        float duration = Mathf.Max(0.1f, snapshot.StartDurationSeconds);
        float actualDuration = success ? duration : Mathf.Max(0.1f, duration * Mathf.Clamp01(targetFill));

        float elapsed = 0f;

        while (elapsed < actualDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / actualDuration);
            float fill = Mathf.Lerp(0f, targetFill, t);

            _progressView?.SetFill(fill);

            yield return null;
        }

        _progressView?.SetFill(targetFill);

        if (success)
        {
            bool consumed = ConsumeSuccessItems(snapshot);

            if (consumed)
            {
                _currentSource?.Ignite(snapshot.BurnMinutes);
            }
            else
            {
                Debug.LogWarning("[FireStarting] Успех выпал, но не удалось потратить предметы для костра/печки.");
            }
        }

        _startRoutine = null;
        CloseAll();
    }

    private FireAttemptSnapshot BuildCurrentSnapshot()
    {
        ItemData igniter = GetSelected(_availableIgniters, _igniterIndex);
        ItemData tinder = GetSelected(_availableTinders, _tinderIndex);
        ItemData fuel = GetSelected(_availableFuels, _fuelIndex);
        ItemData accelerant = GetSelectedOptional(_availableAccelerants, _accelerantIndex);

        bool usesAccelerant = accelerant != null;

        float successChance = CalculateSuccessChance(igniter, tinder, fuel, accelerant);
        float burnMinutes = fuel != null ? fuel.BurnMinutes : 0f;
        float duration = usesAccelerant ? _accelerantStartDurationSeconds : _defaultStartDurationSeconds;

        return new FireAttemptSnapshot
        {
            Igniter = igniter,
            Tinder = tinder,
            Fuel = fuel,
            Accelerant = accelerant,
            UsesAccelerant = usesAccelerant,
            SuccessChance = successChance,
            BurnMinutes = burnMinutes,
            StartDurationSeconds = duration
        };
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

            if (item == null)
            {
                continue;
            }

            if (ContainsSameItem(result, item))
            {
                continue;
            }

            bool available;

            if (isAccelerant && item.UsesCustomAmount)
            {
                available = HasCustomAmount(item, AccelerantAmountCost);
            }
            else
            {
                available = _inventory.ContainsUsableItem(item, 1);
            }

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
        FireAttemptSnapshot snapshot = BuildCurrentSnapshot();

        _igniterView?.Refresh(
            snapshot.Igniter,
            BuildItemAmountText(snapshot.Igniter, 1f)
        );

        _tinderView?.Refresh(
            snapshot.Tinder,
            BuildItemAmountText(snapshot.Tinder, 1f)
        );

        _fuelView?.Refresh(
            snapshot.Fuel,
            BuildItemAmountText(snapshot.Fuel, 1f)
        );

        _accelerantView?.Refresh(
            snapshot.Accelerant,
            BuildItemAmountText(snapshot.Accelerant, AccelerantAmountCost)
        );

        if (_baseChanceText != null)
        {
            _baseChanceText.text = $"{_config.BaseChance:0}%";
        }

        if (_successChanceText != null)
        {
            _successChanceText.text = $"{snapshot.SuccessChance:0}%";
        }

        if (_burnTimeText != null)
        {
            _burnTimeText.text = $"{FormatMinutes(snapshot.BurnMinutes)}";
        }

        if (_startButton != null)
        {
            _startButton.interactable = snapshot.HasRequiredItems;
        }
    }

    private string BuildItemAmountText(ItemData itemData, float requiredAmount)
    {
        if (itemData == null || _inventory == null)
        {
            return string.Empty;
        }

        if (itemData.UsesCustomAmount)
        {
            float currentAmount = _inventory.GetTotalAmount(itemData);

            return $"{FormatAmount(requiredAmount)} л / {FormatAmount(currentAmount)} л";
        }

        int currentCount = _inventory.GetTotalCount(itemData);

        return $"1 из {currentCount}";
    }

    private static string FormatAmount(float amount)
    {
        return amount.ToString("0.##");
    }

    private float CalculateSuccessChance(ItemData igniter, ItemData tinder, ItemData fuel, ItemData accelerant)
    {
        if (_config == null)
        {
            return 0f;
        }

        if (accelerant != null)
        {
            return 100f;
        }

        float chance = _config.BaseChance;

        chance += GetStartChanceBonus(igniter);
        chance += GetStartChanceBonus(tinder);
        chance += GetStartChanceBonus(fuel);

        return Mathf.Clamp(chance, 0f, 100f);
    }

    private static float GetStartChanceBonus(ItemData item)
    {
        return item != null ? item.StartChanceBonus : 0f;
    }

    private bool ConsumeIgniter(ItemData igniter)
    {
        if (igniter == null || _inventory == null)
        {
            return false;
        }

        if (igniter.UsesDurability && !igniter.IsUnbreakable && igniter.Id == "firestriker")
        {
            float durabilityCost = Mathf.Max(0f, igniter.MaxDurability * FlintDurabilityCost01);
            return _inventory.TryConsumeDurabilityFromFirstMatchingItem(igniter, durabilityCost);
        }

        return _inventory.TryRemoveItem(igniter, 1);
    }

    private bool ConsumeSuccessItems(FireAttemptSnapshot snapshot)
    {
        if (_inventory == null || snapshot == null)
        {
            return false;
        }

        if (snapshot.Tinder != null && !_inventory.TryRemoveItem(snapshot.Tinder, 1))
        {
            return false;
        }

        if (snapshot.Fuel != null && !_inventory.TryRemoveItem(snapshot.Fuel, 1))
        {
            return false;
        }

        if (snapshot.Accelerant == null)
        {
            return true;
        }

        if (snapshot.Accelerant.UsesCustomAmount)
        {
            return _inventory.TryConsumeCustomAmountFromFirstMatchingItem(snapshot.Accelerant, AccelerantAmountCost);
        }

        return _inventory.TryRemoveItem(snapshot.Accelerant, 1);
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

            if (!IsSameItem(slot.Item, item))
            {
                continue;
            }

            if (!slot.HasAmount)
            {
                continue;
            }

            if (slot.CurrentAmount >= requiredAmount)
            {
                return true;
            }
        }

        return false;
    }

    private void PreviousIgniter() => StepIndex(ref _igniterIndex, _availableIgniters.Count, -1);
    private void NextIgniter() => StepIndex(ref _igniterIndex, _availableIgniters.Count, 1);

    private void PreviousTinder() => StepIndex(ref _tinderIndex, _availableTinders.Count, -1);
    private void NextTinder() => StepIndex(ref _tinderIndex, _availableTinders.Count, 1);

    private void PreviousFuel() => StepIndex(ref _fuelIndex, _availableFuels.Count, -1);
    private void NextFuel() => StepIndex(ref _fuelIndex, _availableFuels.Count, 1);

    private void PreviousAccelerant() => StepOptionalIndex(ref _accelerantIndex, _availableAccelerants.Count, -1);
    private void NextAccelerant() => StepOptionalIndex(ref _accelerantIndex, _availableAccelerants.Count, 1);

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
        // -1 = "ни одного", 0..count-1 = предметы.
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

    private static bool ContainsSameItem(List<ItemData> items, ItemData item)
    {
        if (items == null || item == null)
        {
            return false;
        }

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

    private sealed class FireAttemptSnapshot
    {
        public ItemData Igniter;
        public ItemData Tinder;
        public ItemData Fuel;
        public ItemData Accelerant;

        public bool UsesAccelerant;
        public float SuccessChance;
        public float BurnMinutes;
        public float StartDurationSeconds;

        public bool HasRequiredItems => Igniter != null && Tinder != null && Fuel != null;
    }
}