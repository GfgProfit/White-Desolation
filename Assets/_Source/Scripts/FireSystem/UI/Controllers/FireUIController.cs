using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class FireUIController : MonoBehaviour
{
    private const float AccelerantAmountCost = 0.3f;
    private const float BurningActionDurationSeconds = 5f;

    [Header("Data")]
    [SerializeField] private FireStartingConfig _config;
    [SerializeField] private FireBurningConfig _burningConfig;

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

    [Header("Burning Fire Window")]
    [SerializeField] private FireBurningOperationWindowView _burningWindowView;

    [Header("Water Items")]
    [SerializeField] private ItemData _meltedWaterItem;
    [SerializeField] private ItemData _boiledWaterItem;

    [Header("Progress Window")]
    [SerializeField] private FireProgressView _progressView;
    [SerializeField, Min(0.1f)] private float _defaultStartDurationSeconds = 5f;
    [SerializeField, Min(0.1f)] private float _accelerantStartDurationSeconds = 2f;
    [SerializeField, Range(0.05f, 0.95f)] private float _failedMinFill = 0.1f;
    [SerializeField, Range(0.05f, 0.95f)] private float _failedMaxFill = 0.85f;

    [Header("Player Lock")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [SerializeField] private GameObject[] _objectsDisableWhileOpen;

    [Inject] private InventoryController _inventory = null;
    [Inject] private IGameTimeAdvancer _gameTimeAdvancer = null;

    private readonly List<ItemData> _availableIgniters = new();
    private readonly List<ItemData> _availableTinders = new();
    private readonly List<ItemData> _availableFuels = new();
    private readonly List<ItemData> _availableAccelerants = new();

    private readonly FireBurningOperationList _burningOperationList = new();

    private readonly FireStartSelectionState _selectionState = new();
    private readonly FireBurningSelectionState _burningSelectionState = new();

    private FireSourceInteractable _currentSource;

    private Coroutine _startRoutine;

    private FireUIControlLockSession _controlLockSession;
    private FireStartWindowPresenter _startWindowPresenter;
    private FireBurningOperationWindowPresenter _burningWindowPresenter;
    private FireStartAvailableItemService _availableItemService;
    private FireStartAttemptService _attemptService;
    private FireStartCompletionService _completionService;
    private FireBurningOperationService _burningOperationService;

    private FireBurningOperationSettings BurningOperationSettings => _burningConfig != null ? _burningConfig.Settings : FireBurningConfig.DefaultSettings;
}
