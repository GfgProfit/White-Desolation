using System.Collections.Generic;
using UnityEngine;

public static class CursorLockService
{
    private static readonly HashSet<object> Owners = new();

    private static bool _hasSnapshot;
    private static bool _initialVisible;
    private static CursorLockMode _initialLockState;

    public static void ShowCursor(object owner)
    {
        if (owner == null)
        {
            return;
        }

        if (Owners.Count == 0 && !_hasSnapshot)
        {
            _initialVisible = Cursor.visible;
            _initialLockState = Cursor.lockState;
            _hasSnapshot = true;
        }

        Owners.Add(owner);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void ReleaseCursor(object owner)
    {
        if (owner == null)
        {
            return;
        }

        if (!Owners.Remove(owner))
        {
            return;
        }

        if (Owners.Count > 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        RestoreInitialState();
    }

    public static void ReleaseOwner(object owner)
    {
        ReleaseCursor(owner);
    }

    public static bool IsLockedByAnyOwner => Owners.Count > 0;

    private static void RestoreInitialState()
    {
        if (_hasSnapshot)
        {
            Cursor.visible = _initialVisible;
            Cursor.lockState = _initialLockState;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        _hasSnapshot = false;
    }
}