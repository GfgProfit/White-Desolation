using System;

public interface IGameTimeAdvanceNotifier
{
    event Action<float> OnGameMinutesAdvanced;
}
