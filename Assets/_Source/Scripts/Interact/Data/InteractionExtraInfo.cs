using UnityEngine;

public enum InteractionExtraInfoState
{
    None,
    Empty,
    Inspected,
    DryProgress,
    Custom
}

public sealed class InteractionExtraInfo : MonoBehaviour, IInteractionExtraInfoProvider
{
    [SerializeField] private InteractionExtraInfoState _state = InteractionExtraInfoState.None;

    [SerializeField, Range(0f, 100f)] private float _dryProgressPercent;
    [SerializeField] private string _customText;

    public InteractionExtraInfoState State => _state;
    public float DryProgressPercent => _dryProgressPercent;

    public bool TryGetExtraInfo(out string infoText)
    {
        infoText = string.Empty;

        switch (_state)
        {
            case InteractionExtraInfoState.Empty:
                infoText = "Пусто";
                return true;

            case InteractionExtraInfoState.Inspected:
                infoText = "Осмотрено";
                return true;

            case InteractionExtraInfoState.DryProgress:
                infoText = $"Просушено {Mathf.RoundToInt(_dryProgressPercent)}%";
                return true;

            case InteractionExtraInfoState.Custom:
                if (string.IsNullOrWhiteSpace(_customText))
                {
                    return false;
                }

                infoText = _customText;
                return true;

            default:
                return false;
        }
    }

    public void Clear()
    {
        _state = InteractionExtraInfoState.None;
        _dryProgressPercent = 0f;
        _customText = string.Empty;
    }

    public void SetEmpty()
    {
        _state = InteractionExtraInfoState.Empty;
    }

    public void SetInspected()
    {
        _state = InteractionExtraInfoState.Inspected;
    }

    public void SetDryProgress(float percent)
    {
        _state = InteractionExtraInfoState.DryProgress;
        _dryProgressPercent = Mathf.Clamp(percent, 0f, 100f);
    }

    public void SetCustom(string text)
    {
        _state = InteractionExtraInfoState.Custom;
        _customText = text;
    }
}