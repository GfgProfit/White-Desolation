using System.Collections.Generic;
using UnityEngine;

public sealed class SavedWorldItemSpawnService
{
    private const string DebugPrefix = "[WorldItemRestore]";

    private readonly ISaveableObjectProvider _saveableObjectProvider;
    private readonly WorldItem _fallbackWorldItemPrefab;

    public SavedWorldItemSpawnService(ISaveableObjectProvider saveableObjectProvider, WorldItem fallbackWorldItemPrefab = null)
    {
        _saveableObjectProvider = saveableObjectProvider ?? new SceneSaveableObjectProvider();
        _fallbackWorldItemPrefab = fallbackWorldItemPrefab;
    }

    public void RestoreRuntimeWorldItems(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.WorldItems == null)
        {
            return;
        }

        if (context == null || !context.TryGet(out ItemDatabase itemDatabase))
        {
            Debug.LogWarning($"{DebugPrefix} Cannot restore runtime world items without ItemDatabase.");
            return;
        }

        Dictionary<string, WorldItemSaveData> statesBySaveId = BuildStatesBySaveId(saveData.WorldItems);

        DestroyRuntimeItemsMissingFromSave(statesBySaveId);

        HashSet<string> existingSaveIds = BuildExistingWorldItemSaveIds();

        for (int i = 0; i < saveData.WorldItems.Count; i++)
        {
            WorldItemSaveData state = saveData.WorldItems[i];

            if (!ShouldSpawn(state, existingSaveIds))
            {
                continue;
            }

            Spawn(state, itemDatabase);
            existingSaveIds.Add(state.SaveId);
        }
    }

    private Dictionary<string, WorldItemSaveData> BuildStatesBySaveId(List<WorldItemSaveData> states)
    {
        Dictionary<string, WorldItemSaveData> result = new();

        for (int i = 0; i < states.Count; i++)
        {
            WorldItemSaveData state = states[i];

            if (state == null || string.IsNullOrWhiteSpace(state.SaveId))
            {
                continue;
            }

            result[state.SaveId] = state;
        }

        return result;
    }

    private void DestroyRuntimeItemsMissingFromSave(Dictionary<string, WorldItemSaveData> statesBySaveId)
    {
        WorldItem[] worldItems = _saveableObjectProvider.FindAll<WorldItem>();

        for (int i = 0; i < worldItems.Length; i++)
        {
            WorldItem worldItem = worldItems[i];

            if (worldItem == null || !worldItem.IsRuntimeSpawned)
            {
                continue;
            }

            if (!statesBySaveId.TryGetValue(worldItem.SaveId, out WorldItemSaveData state) || state.PickedUp)
            {
                Object.Destroy(worldItem.gameObject);
            }
        }
    }

    private HashSet<string> BuildExistingWorldItemSaveIds()
    {
        HashSet<string> saveIds = new();
        WorldItem[] worldItems = _saveableObjectProvider.FindAll<WorldItem>();

        for (int i = 0; i < worldItems.Length; i++)
        {
            WorldItem worldItem = worldItems[i];

            if (worldItem == null || string.IsNullOrWhiteSpace(worldItem.SaveId))
            {
                continue;
            }

            saveIds.Add(worldItem.SaveId);
        }

        return saveIds;
    }

    private bool ShouldSpawn(WorldItemSaveData state, HashSet<string> existingSaveIds)
    {
        return state != null
            && !state.PickedUp
            && !string.IsNullOrWhiteSpace(state.SaveId)
            && !existingSaveIds.Contains(state.SaveId);
    }

    private void Spawn(WorldItemSaveData state, ItemDatabase itemDatabase)
    {
        if (!itemDatabase.TryGetItem(state.ItemId, out ItemData itemData))
        {
            Debug.LogWarning($"{DebugPrefix} Cannot restore world item '{state.SaveId}'. Unknown item id '{state.ItemId}'.");
            return;
        }

        WorldItem prefab = ResolvePrefab(itemData);

        if (prefab == null)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot restore '{itemData.DisplayName}'. No world prefab configured.");
            return;
        }

        Vector3 position = state.Position.ToVector3();
        Quaternion rotation = state.Rotation.ToQuaternion();
        WorldItem worldItem = Object.Instantiate(prefab, position, rotation);

        float? currentAmount = state.OverrideCurrentAmount ? state.CurrentAmount : null;
        float? currentDurability = state.OverrideCurrentDurability ? state.CurrentDurability : null;

        worldItem.InitializeRuntime(itemData, Mathf.Max(1, state.Count), currentAmount, currentDurability, regenerateSaveId: false, saveId: state.SaveId);
        SceneInstaller.Container?.InjectGameObject(worldItem.gameObject, true);
    }

    private WorldItem ResolvePrefab(ItemData itemData)
    {
        if (itemData != null && itemData.WorldPrefab != null)
        {
            return itemData.WorldPrefab;
        }

        return _fallbackWorldItemPrefab;
    }
}
