using System.Collections.Generic;
using UnityEngine;

public static class CursorLockService
{
    private static readonly HashSet<object> Owners = new();

    private static bool _hasSnapshot;
    private static bool _initialVisible;
    private static CursorLockMode _initialLockState;
    private static int _gameplayInputBlockedUntilFrame = -1;

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

        BlockGameplayInputForCurrentFrame();
        RestoreInitialState();
    }

    public static void ReleaseOwner(object owner)
    {
        ReleaseCursor(owner);
    }

    public static void ForceUnlock()
    {
        Owners.Clear();
        _hasSnapshot = false;
        BlockGameplayInputForCurrentFrame();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public static bool IsLockedByAnyOwner => Owners.Count > 0;
    public static bool IsGameplayInputBlocked => Owners.Count > 0 || Time.frameCount <= _gameplayInputBlockedUntilFrame;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        Owners.Clear();
        _hasSnapshot = false;
        _initialVisible = false;
        _initialLockState = CursorLockMode.None;
        _gameplayInputBlockedUntilFrame = -1;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private static void BlockGameplayInputForCurrentFrame()
    {
        _gameplayInputBlockedUntilFrame = Mathf.Max(_gameplayInputBlockedUntilFrame, Time.frameCount);
    }

    private static void RestoreInitialState()
    {
        if (_hasSnapshot)
        {
            Cursor.visible = _initialVisible;
            Cursor.lockState = _initialLockState;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        _hasSnapshot = false;
    }
}
