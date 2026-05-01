using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerControlLockSession : IDisposable
{
    private readonly object _owner;
    private readonly IReadOnlyList<Behaviour> _behaviours;
    private readonly IReadOnlyList<GameObject> _gameObjects;
    private bool _isLocked;

    public bool IsLocked => _isLocked;

    public PlayerControlLockSession(object owner, IReadOnlyList<Behaviour> behaviours, IReadOnlyList<GameObject> gameObjects)
    {
        _owner = owner;
        _behaviours = behaviours;
        _gameObjects = gameObjects;
    }

    public void Lock()
    {
        if (_isLocked || _owner == null)
        {
            return;
        }

        PlayerControlLockService.Lock(_owner, _behaviours, _gameObjects);
        _isLocked = true;
    }

    public void Unlock()
    {
        if (!_isLocked || _owner == null)
        {
            return;
        }

        PlayerControlLockService.Unlock(_owner, _behaviours, _gameObjects);
        _isLocked = false;
    }

    public void Release()
    {
        if (_owner == null)
        {
            _isLocked = false;
            return;
        }

        PlayerControlLockService.ReleaseOwner(_owner);
        _isLocked = false;
    }

    public void Dispose()
    {
        Release();
    }
}
