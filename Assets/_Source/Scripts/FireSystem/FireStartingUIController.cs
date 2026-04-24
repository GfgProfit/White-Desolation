using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireStartingUIController : MonoBehaviour
{
    private const float AccelerantAmountCost = 0.3f;
    private const float FlintDurabilityCost = 2f;

    [Header("Data")]
    [SerializeField] private FireItemsConfig _config;

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

    [Header("Player Lock")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [SerializeField] private GameObject[] _objectsDisableWhileOpen;

    [Inject] private readonly InventoryController _inventory;

    private FireSourceInteractable _currentSource;

    private void Awake()
    {
        _closeButton.onClick.AddListener(() => CloseAll());
    }

    public void OpenFireStarting(FireSourceInteractable source)
    {
        _currentSource = source;
        SetPlayerControlsEnabled(false);
        SetObjectsEnabled(false);
        _progressView.Hide();
        SetStartVisible(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseAll()
    {
        SetStartVisible(false);
        _progressView.Hide();
        SetPlayerControlsEnabled(true);
        SetObjectsEnabled(true);
        _currentSource = null;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SetStartVisible(bool visible)
    {
        if (_startRoot != null)
        {
            _startRoot.SetActive(visible);
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

    private string FormatMinutes(float minutes)
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
