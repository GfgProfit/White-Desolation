using UnityEngine;

[RequireComponent(typeof(SaveId))]
public sealed partial class FireSourceInteractable : MonoBehaviour, IInteractable, IInteractHoverInfo, ISaveable
{
    private const string DebugPrefix = "[FireSource]";

    [Header("Display")]
    [SerializeField] private string _displayName = "Печка";

    [Header("Runtime")]
    [SerializeField] private bool _isBurning;
    [SerializeField, Min(0f)] private float _remainingBurnGameMinutes;

    [Header("Temperature")]
    [SerializeField] private float _temperatureCelsius = 0f;

    [Header("Save")]
    [SerializeField] private SaveId _saveId;

    [Inject] private IFireSourceInteractionHandler _interactionHandler = null;
    [Inject] private IGameTimeConverter _gameTimeConverter = null;
    [Inject] private IGameTimeAdvanceNotifier _gameTimeAdvanceNotifier = null;

    private IGameTimeAdvanceNotifier _subscribedGameTimeNotifier;

    public string SaveId => _saveId != null ? _saveId.Id : string.Empty;
    public string DisplayName => _displayName;
    public bool IsBurning => _isBurning;
    public float RemainingBurnSeconds => GameMinutesToRealSeconds(_remainingBurnGameMinutes);
    public float RemainingBurnMinutes => _remainingBurnGameMinutes;
    public float TemperatureCelsius => _temperatureCelsius;

    private void Reset()
    {
        CacheSaveId();
    }

    private void Awake()
    {
        CacheSaveId();
    }

    private void OnEnable()
    {
        SubscribeToGameTime();
    }

    private void Start()
    {
        SubscribeToGameTime();
    }

    private void OnDisable()
    {
        UnsubscribeFromGameTime();
    }

    private void CacheSaveId()
    {
        if (_saveId == null)
        {
            _saveId = GetComponent<SaveId>();
        }
    }
}
