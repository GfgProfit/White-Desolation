using UnityEngine;

public sealed class FireUIControlLockSession
{
    private readonly object _owner;
    private readonly PlayerControlLockSession _controlLockSession;

    public FireUIControlLockSession(object owner, Behaviour[] behavioursToDisable, GameObject[] objectsToDisable)
    {
        _owner = owner;
        _controlLockSession = PlayerControlLockService.CreateSession(owner, behavioursToDisable, objectsToDisable);
    }

    public void Open()
    {
        _controlLockSession.Lock();
        CursorLockService.ShowCursor(_owner);
    }

    public void Close()
    {
        _controlLockSession.Unlock();
        CursorLockService.ReleaseCursor(_owner);
    }

    public void Release()
    {
        _controlLockSession.Release();
        CursorLockService.ReleaseOwner(_owner);
    }
}
