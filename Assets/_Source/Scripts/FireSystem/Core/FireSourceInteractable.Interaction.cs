using UnityEngine;

public sealed partial class FireSourceInteractable
{
    public void Interact()
    {
        if (_interactionHandler == null)
        {
            Debug.LogWarning($"{DebugPrefix} Fire interaction handler is missing.");
            return;
        }

        _interactionHandler.InteractWith(this);
    }

    public InteractionHoverInfo GetHoverInfo()
    {
        InteractionHoverInfo info = new()
        {
            InteractionText = _displayName
        };

        if (!_isBurning)
        {
            return info;
        }

        info.TimeText = FireSourceDisplayFormatter.FormatBurnTime(_remainingBurnGameMinutes);
        info.TemperatureText = FireSourceDisplayFormatter.FormatTemperature(_temperatureCelsius);

        return info;
    }
}
