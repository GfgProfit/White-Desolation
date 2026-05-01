public sealed partial class FireSourceInteractable
{
    private void SubscribeToGameTime()
    {
        if (_gameTimeAdvanceNotifier == null || _subscribedGameTimeNotifier == _gameTimeAdvanceNotifier)
        {
            return;
        }

        UnsubscribeFromGameTime();
        _subscribedGameTimeNotifier = _gameTimeAdvanceNotifier;
        _subscribedGameTimeNotifier.OnGameMinutesAdvanced += HandleGameMinutesAdvanced;
    }

    private void UnsubscribeFromGameTime()
    {
        if (_subscribedGameTimeNotifier == null)
        {
            return;
        }

        _subscribedGameTimeNotifier.OnGameMinutesAdvanced -= HandleGameMinutesAdvanced;
        _subscribedGameTimeNotifier = null;
    }

    private void HandleGameMinutesAdvanced(float gameMinutes)
    {
        ConsumeBurnTime(gameMinutes);
    }
}
