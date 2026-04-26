using System.Collections.Generic;
using UnityEngine;

public static class PlayerControlLockService
{
    private sealed class BehaviourLockState
    {
        public readonly bool InitialEnabled;
        public readonly HashSet<object> Owners = new();

        public BehaviourLockState(bool initialEnabled)
        {
            InitialEnabled = initialEnabled;
        }
    }

    private sealed class GameObjectLockState
    {
        public readonly bool InitialActiveSelf;
        public readonly HashSet<object> Owners = new();

        public GameObjectLockState(bool initialActiveSelf)
        {
            InitialActiveSelf = initialActiveSelf;
        }
    }

    private static readonly Dictionary<Behaviour, BehaviourLockState> BehaviourStates = new();
    private static readonly Dictionary<GameObject, GameObjectLockState> GameObjectStates = new();

    public static void Lock(object owner, IReadOnlyList<Behaviour> behaviours, IReadOnlyList<GameObject> gameObjects)
    {
        if (owner == null)
        {
            return;
        }

        LockBehaviours(owner, behaviours);
        LockGameObjects(owner, gameObjects);
    }

    public static void Unlock(object owner, IReadOnlyList<Behaviour> behaviours, IReadOnlyList<GameObject> gameObjects)
    {
        if (owner == null)
        {
            return;
        }

        UnlockBehaviours(owner, behaviours);
        UnlockGameObjects(owner, gameObjects);
    }

    public static void LockBehaviours(object owner, IReadOnlyList<Behaviour> behaviours)
    {
        if (owner == null || behaviours == null)
        {
            return;
        }

        for (int i = 0; i < behaviours.Count; i++)
        {
            Behaviour target = behaviours[i];

            if (target == null)
            {
                continue;
            }

            if (!BehaviourStates.TryGetValue(target, out BehaviourLockState state))
            {
                state = new BehaviourLockState(target.enabled);
                BehaviourStates.Add(target, state);
            }

            state.Owners.Add(owner);
            target.enabled = false;
        }
    }

    public static void UnlockBehaviours(object owner, IReadOnlyList<Behaviour> behaviours)
    {
        if (owner == null || behaviours == null)
        {
            return;
        }

        for (int i = 0; i < behaviours.Count; i++)
        {
            Behaviour target = behaviours[i];

            if (target == null)
            {
                continue;
            }

            if (!BehaviourStates.TryGetValue(target, out BehaviourLockState state))
            {
                continue;
            }

            state.Owners.Remove(owner);

            if (state.Owners.Count > 0)
            {
                continue;
            }

            target.enabled = state.InitialEnabled;
            BehaviourStates.Remove(target);
        }
    }

    public static void LockGameObjects(object owner, IReadOnlyList<GameObject> gameObjects)
    {
        if (owner == null || gameObjects == null)
        {
            return;
        }

        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject target = gameObjects[i];

            if (target == null)
            {
                continue;
            }

            if (!GameObjectStates.TryGetValue(target, out GameObjectLockState state))
            {
                state = new GameObjectLockState(target.activeSelf);
                GameObjectStates.Add(target, state);
            }

            state.Owners.Add(owner);
            target.SetActive(false);
        }
    }

    public static void UnlockGameObjects(object owner, IReadOnlyList<GameObject> gameObjects)
    {
        if (owner == null || gameObjects == null)
        {
            return;
        }

        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject target = gameObjects[i];

            if (target == null)
            {
                continue;
            }

            if (!GameObjectStates.TryGetValue(target, out GameObjectLockState state))
            {
                continue;
            }

            state.Owners.Remove(owner);

            if (state.Owners.Count > 0)
            {
                continue;
            }

            target.SetActive(state.InitialActiveSelf);
            GameObjectStates.Remove(target);
        }
    }

    public static void ReleaseOwner(object owner)
    {
        if (owner == null)
        {
            return;
        }

        ReleaseBehaviourOwner(owner);
        ReleaseGameObjectOwner(owner);
    }

    private static void ReleaseBehaviourOwner(object owner)
    {
        List<Behaviour> targetsToRestore = new();

        foreach (KeyValuePair<Behaviour, BehaviourLockState> pair in BehaviourStates)
        {
            Behaviour target = pair.Key;
            BehaviourLockState state = pair.Value;

            if (target == null)
            {
                targetsToRestore.Add(target);
                continue;
            }

            state.Owners.Remove(owner);

            if (state.Owners.Count == 0)
            {
                targetsToRestore.Add(target);
            }
        }

        for (int i = 0; i < targetsToRestore.Count; i++)
        {
            Behaviour target = targetsToRestore[i];

            if (target != null && BehaviourStates.TryGetValue(target, out BehaviourLockState state))
            {
                target.enabled = state.InitialEnabled;
            }

            BehaviourStates.Remove(target);
        }
    }

    private static void ReleaseGameObjectOwner(object owner)
    {
        List<GameObject> targetsToRestore = new();

        foreach (KeyValuePair<GameObject, GameObjectLockState> pair in GameObjectStates)
        {
            GameObject target = pair.Key;
            GameObjectLockState state = pair.Value;

            if (target == null)
            {
                targetsToRestore.Add(target);
                continue;
            }

            state.Owners.Remove(owner);

            if (state.Owners.Count == 0)
            {
                targetsToRestore.Add(target);
            }
        }

        for (int i = 0; i < targetsToRestore.Count; i++)
        {
            GameObject target = targetsToRestore[i];

            if (target != null && GameObjectStates.TryGetValue(target, out GameObjectLockState state))
            {
                target.SetActive(state.InitialActiveSelf);
            }

            GameObjectStates.Remove(target);
        }
    }
}