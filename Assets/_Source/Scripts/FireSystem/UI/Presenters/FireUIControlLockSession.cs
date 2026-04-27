using UnityEngine;

public sealed class FireUIControlLockSession
{
    private readonly object _owner;
    private readonly Behaviour[] _behavioursToDisable;
    private readonly GameObject[] _objectsToDisable;

    public FireUIControlLockSession(object owner, Behaviour[] behavioursToDisable, GameObject[] objectsToDisable)
    {
        _owner = owner;
        _behavioursToDisable = behavioursToDisable;
        _objectsToDisable = objectsToDisable;
    }

    public void Open()
    {
        PlayerControlLockService.LockBehaviours(_owner, _behavioursToDisable);
        PlayerControlLockService.LockGameObjects(_owner, _objectsToDisable);
        CursorLockService.ShowCursor(_owner);
    }

    public void Close()
    {
        PlayerControlLockService.UnlockBehaviours(_owner, _behavioursToDisable);
        PlayerControlLockService.UnlockGameObjects(_owner, _objectsToDisable);
        CursorLockService.ReleaseCursor(_owner);
    }

    public void Release()
    {
        PlayerControlLockService.ReleaseOwner(_owner);
        CursorLockService.ReleaseOwner(_owner);
    }
}