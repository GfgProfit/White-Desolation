#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ProjectIdValidator
{
    private const string MenuPath = "Tools/Validation/Validate Project IDs";

    static ProjectIdValidator()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem(MenuPath)]
    public static void ValidateProjectIds()
    {
        int issueCount = ValidateItemDataIds();
        issueCount += ValidateOpenSceneSaveIds();

        if (issueCount == 0)
        {
            Debug.Log("[Validation] Project IDs are valid.");
        }
        else
        {
            Debug.LogWarning($"[Validation] Project ID validation found {issueCount} issue(s).");
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
        {
            return;
        }

        ValidateProjectIds();
    }

    private static int ValidateItemDataIds()
    {
        Dictionary<string, string> pathsById = new();
        int issueCount = 0;

        string[] guids = AssetDatabase.FindAssets("t:ItemData");

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);

            if (item == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.Id))
            {
                Debug.LogWarning($"[Validation] ItemData has empty Id: {path}", item);
                issueCount++;
                continue;
            }

            if (pathsById.TryGetValue(item.Id, out string existingPath))
            {
                Debug.LogWarning($"[Validation] Duplicate ItemData.Id '{item.Id}': {existingPath} and {path}", item);
                issueCount++;
                continue;
            }

            pathsById.Add(item.Id, path);
        }

        return issueCount;
    }

    private static int ValidateOpenSceneSaveIds()
    {
        Dictionary<string, SaveId> saveIdsById = new();
        int issueCount = 0;

        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
            {
                continue;
            }

            GameObject[] roots = scene.GetRootGameObjects();

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                issueCount += ValidateSaveIdsInRoot(roots[rootIndex], saveIdsById);
            }
        }

        return issueCount;
    }

    private static int ValidateSaveIdsInRoot(GameObject root, Dictionary<string, SaveId> saveIdsById)
    {
        if (root == null)
        {
            return 0;
        }

        SaveId[] saveIds = root.GetComponentsInChildren<SaveId>(true);
        int issueCount = 0;

        for (int i = 0; i < saveIds.Length; i++)
        {
            SaveId saveId = saveIds[i];

            if (saveId == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(saveId.Id))
            {
                Debug.LogWarning($"[Validation] SaveId is empty on '{GetHierarchyPath(saveId.transform)}'.", saveId);
                issueCount++;
                continue;
            }

            if (saveIdsById.TryGetValue(saveId.Id, out SaveId existing))
            {
                Debug.LogWarning($"[Validation] Duplicate SaveId '{saveId.Id}': '{GetHierarchyPath(existing.transform)}' and '{GetHierarchyPath(saveId.transform)}'.", saveId);
                issueCount++;
                continue;
            }

            saveIdsById.Add(saveId.Id, saveId);
        }

        return issueCount;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return "<missing>";
        }

        string path = transform.name;
        Transform parent = transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
#endif
