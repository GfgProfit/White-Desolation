using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireUIController : MonoBehaviour
{
    private const float AccelerantAmountCost = 0.3f;

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

    [Header("Player Lock")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [SerializeField] private GameObject[] _objectsDisableWhileOpen;

    [Inject] private InventoryController _inventory;

    private readonly List<ItemData> _availableIgniters = new List<ItemData>();
    private readonly List<ItemData> _availableTinders = new List<ItemData>();
    private readonly List<ItemData> _availableFuels = new List<ItemData>();
    private readonly List<ItemData> _availableAccelerants = new List<ItemData>();

    private readonly FireStartSelectionState _selectionState = new FireStartSelectionState();

    private FireSourceInteractable _currentSource;

    private Coroutine _startRoutine;

    private FireUIControlLockSession _controlLockSession;
    private FireStartWindowPresenter _startWindowPresenter;
    private FireStartAvailableItemService _availableItemService;
    private FireStartAttemptService _attemptService;
    private FireStartCompletionService _completionService;

    private void Awake()
    {
        _controlLockSession = new FireUIControlLockSession(this, _disableWhileOpen, _objectsDisableWhileOpen);
        _startWindowPresenter = new FireStartWindowPresenter(_startRoot, _igniterView, _tinderView, _fuelView, _accelerantView, _baseChanceText, _successChanceText, _burnTimeText, _startButton, _closeButton);
        _availableItemService = new FireStartAvailableItemService(_inventory, AccelerantAmountCost);
        _attemptService = new FireStartAttemptService(_inventory, _failedMinFill, _failedMaxFill);
        _completionService = new FireStartCompletionService(_inventory);

        _startWindowPresenter.Bind(PreviousIgniter, NextIgniter, PreviousTinder, NextTinder, PreviousFuel, NextFuel, PreviousAccelerant, NextAccelerant, StartFireAttempt, CloseAll);
        _startWindowPresenter.Hide();

        _progressView?.Hide();
    }

    private void OnDestroy()
    {
        _controlLockSession?.Release();
    }

    private void OnDisable()
    {
        if (_startRoutine != null)
        {
            StopCoroutine(_startRoutine);
            _startRoutine = null;
        }

        _currentSource = null;

        _controlLockSession?.Release();
    }

    private void Update()
    {
        if (_startWindowPresenter == null || !_startWindowPresenter.IsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            CloseAll();
        }
    }

    public void OpenFireStarting(FireSourceInteractable source)
    {
        if (_currentSource != null || _startRoutine != null)
        {
            return;
        }

        if (source == null)
        {
            return;
        }

        if (source.IsBurning)
        {
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

        _progressView?.Hide();

        _controlLockSession.Open();

        _startWindowPresenter.Show();
    }

    public void CloseAll()
    {
        if (_startRoutine != null)
        {
            StopCoroutine(_startRoutine);
            _startRoutine = null;
        }

        _startWindowPresenter.Hide();

        _progressView?.Hide();

        _controlLockSession?.Close();

        _currentSource = null;

        CursorLockService.ReleaseCursor(this);
    }

    private void StartFireAttempt()
    {
        if (_startRoutine != null)
        {
            return;
        }

        FireStartPlan plan = BuildCurrentPlan();
        FireStartAttemptResult result = _attemptService.Begin(plan);

        if (!result.Started)
        {
            HandleFailedStartAttempt(result);
            return;
        }

        _startWindowPresenter.Hide();

        _progressView?.Show("разводим огонь");

        _startRoutine = StartCoroutine(FireProgressRoutine(plan, result.Success, result.TargetFill));
    }

    private void HandleFailedStartAttempt(FireStartAttemptResult result)
    {
        if (result.Status == FireStartAttemptStatus.MissingRequiredItems)
        {
            Debug.LogWarning("[FireStarting] Нужны воспламенитель, трут и топливо.");
            RefreshAllViews();
            return;
        }

        if (result.Status == FireStartAttemptStatus.FailedToPayAttemptCost)
        {
            Debug.LogWarning("[FireStarting] Не удалось потратить воспламенитель.");
            RebuildAvailableItems();
            ResetSelectionIndexes();
            RefreshAllViews();
        }
    }

    private IEnumerator FireProgressRoutine(FireStartPlan plan, bool success, float targetFill)
    {
        float duration = Mathf.Max(0.1f, plan.StartDurationSeconds);
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

        _completionService.Complete(plan, _currentSource, success);

        _startRoutine = null;

        CloseAll();
    }

    private FireStartPlan BuildCurrentPlan()
    {
        ItemData igniter = _selectionState.GetIgniter(_availableIgniters);
        ItemData tinder = _selectionState.GetTinder(_availableTinders);
        ItemData fuel = _selectionState.GetFuel(_availableFuels);
        ItemData accelerant = _selectionState.GetAccelerant(_availableAccelerants);

        return FireStartPlanBuilder.Build(_config, igniter, tinder, fuel, accelerant, _defaultStartDurationSeconds, _accelerantStartDurationSeconds, AccelerantAmountCost);
    }

    private void RebuildAvailableItems()
    {
        _availableItemService.Rebuild(_config, _availableIgniters, _availableTinders, _availableFuels, _availableAccelerants);
    }

    private void ResetSelectionIndexes()
    {
        _selectionState.Reset(_availableIgniters.Count, _availableTinders.Count, _availableFuels.Count);
    }

    private void RefreshAllViews()
    {
        FireStartPlan plan = BuildCurrentPlan();
        _startWindowPresenter.Refresh(plan, _config, _inventory, AccelerantAmountCost);
    }

    private void PreviousIgniter()
    {
        _selectionState.PreviousIgniter(_availableIgniters.Count);
        RefreshAllViews();
    }

    private void NextIgniter()
    {
        _selectionState.NextIgniter(_availableIgniters.Count);
        RefreshAllViews();
    }

    private void PreviousTinder()
    {
        _selectionState.PreviousTinder(_availableTinders.Count);
        RefreshAllViews();
    }

    private void NextTinder()
    {
        _selectionState.NextTinder(_availableTinders.Count);
        RefreshAllViews();
    }

    private void PreviousFuel()
    {
        _selectionState.PreviousFuel(_availableFuels.Count);
        RefreshAllViews();
    }

    private void NextFuel()
    {
        _selectionState.NextFuel(_availableFuels.Count);
        RefreshAllViews();
    }

    private void PreviousAccelerant()
    {
        _selectionState.PreviousAccelerant(_availableAccelerants.Count);
        RefreshAllViews();
    }

    private void NextAccelerant()
    {
        _selectionState.NextAccelerant(_availableAccelerants.Count);
        RefreshAllViews();
    }
}