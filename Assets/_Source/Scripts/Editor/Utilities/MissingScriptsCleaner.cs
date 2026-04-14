using UnityEngine;
using UnityEditor;

public class MissingScriptsCleaner : EditorWindow
{
    [MenuItem("Tools/Clean Missing Scripts in Scene")]
    static void CleanMissingScriptsInScene()
    {
        int goCount = 0;
        int componentsRemoved = 0;

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go)))
            {
                int countBefore = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

                if (countBefore > 0)
                {
                    Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

                    componentsRemoved += countBefore;
                    goCount++;
                }
            }
        }

        Debug.Log($"Deleted {componentsRemoved} Missing Script from {goCount} objects.");
    }
}