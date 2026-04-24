using UnityEngine;

public sealed class FireSourceInteractable : MonoBehaviour, IInteractable, IInteractHoverInfo
{
    [Header("Display")]
    [SerializeField] private string _displayName = "Печка";

    [Header("Runtime")]
    [SerializeField] private bool _isBurning;
    [SerializeField, Min(0f)] private float _remainingBurnSeconds;

    [Inject] private readonly FireStartingUIController _fireStartingUI;

    public string DisplayName => _displayName;
    public bool IsBurning => _isBurning;
    public float RemainingBurnSeconds => _remainingBurnSeconds;
    public float RemainingBurnMinutes => _remainingBurnSeconds / 60f;

    private void Update()
    {
        if (!_isBurning)
        {
            return;
        }

        _remainingBurnSeconds = Mathf.Max(0f, _remainingBurnSeconds - Time.deltaTime);

        if (_remainingBurnSeconds <= 0f)
        {
            _isBurning = false;
        }
    }

    public void Interact()
    {
        if (_isBurning)
        {
            return;
        }

        _fireStartingUI.OpenFireStarting(this);
    }

    public string GetHoverText()
    {
        if (!_isBurning)
        {
            return _displayName;
        }

        return $"{_displayName}\nВремя горения: {FormatTime(_remainingBurnSeconds)}\nТемпература: 0.0 C";
    }

    public void Ignite(float burnMinutes)
    {
        _remainingBurnSeconds = Mathf.Max(0f, burnMinutes * 60f);
        _isBurning = _remainingBurnSeconds > 0f;
    }

    private static string FormatTime(float seconds)
    {
        int totalMinutes = Mathf.CeilToInt(Mathf.Max(0f, seconds) / 60f);
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        if (hours > 0)
        {
            return $"{hours} ч {minutes:00} мин";
        }

        return $"{minutes} мин";
    }
}
