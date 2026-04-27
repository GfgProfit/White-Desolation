using UnityEngine;

public sealed class InventoryWindowStateController
{
    private readonly object _owner;
    private readonly GameObject _root;
    private readonly Behaviour[] _disableWhileOpen;
    private readonly GameObject[] _objectDisableWhileOpen;

    public bool IsOpen { get; private set; }

    public InventoryWindowStateController(object owner, GameObject root, Behaviour[] disableWhileOpen, GameObject[] objectDisableWhileOpen)
    {
        _owner = owner;
        _root = root;
        _disableWhileOpen = disableWhileOpen;
        _objectDisableWhileOpen = objectDisableWhileOpen;
    }

    public void InitializeClosed()
    {
        IsOpen = false;

        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    public bool Open()
    {
        if (IsOpen)
        {
            return false;
        }

        IsOpen = true;

        if (_root != null)
        {
            _root.SetActive(true);
        }

        LockBlockedBehaviours();
        LockBlockedObjects();
        CursorLockService.ShowCursor(_owner);

        return true;
    }

    public bool Close()
    {
        if (!IsOpen)
        {
            return false;
        }

        IsOpen = false;

        if (_root != null)
        {
            _root.SetActive(false);
        }

        UnlockBlockedBehaviours();
        UnlockBlockedObjects();
        CursorLockService.ReleaseCursor(_owner);

        return true;
    }

    public void ReleaseOwner()
    {
        IsOpen = false;

        PlayerControlLockService.ReleaseOwner(_owner);
        CursorLockService.ReleaseOwner(_owner);
    }

    private void LockBlockedBehaviours()
    {
        PlayerControlLockService.LockBehaviours(_owner, _disableWhileOpen);
    }

    private void UnlockBlockedBehaviours()
    {
        PlayerControlLockService.UnlockBehaviours(_owner, _disableWhileOpen);
    }

    private void LockBlockedObjects()
    {
        PlayerControlLockService.LockGameObjects(_owner, _objectDisableWhileOpen);
    }

    private void UnlockBlockedObjects()
    {
        PlayerControlLockService.UnlockGameObjects(_owner, _objectDisableWhileOpen);
    }
}