using UnityEngine;

public sealed class InventoryWindowStateController
{
    private readonly object _owner;
    private readonly GameObject _root;
    private readonly PlayerControlLockSession _controlLockSession;

    public bool IsOpen { get; private set; }

    public InventoryWindowStateController(object owner, GameObject root, Behaviour[] disableWhileOpen, GameObject[] objectDisableWhileOpen)
    {
        _owner = owner;
        _root = root;
        _controlLockSession = PlayerControlLockService.CreateSession(owner, disableWhileOpen, objectDisableWhileOpen);
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

        LockBlockedControls();
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

        UnlockBlockedControls();
        CursorLockService.ReleaseCursor(_owner);

        return true;
    }

    public void ReleaseOwner()
    {
        IsOpen = false;

        _controlLockSession.Release();
        CursorLockService.ReleaseOwner(_owner);
    }

    private void LockBlockedControls()
    {
        _controlLockSession.Lock();
    }

    private void UnlockBlockedControls()
    {
        _controlLockSession.Unlock();
    }
}
