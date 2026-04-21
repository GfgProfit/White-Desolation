#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ItemDataProjectWindowIcons
{
    static ItemDataProjectWindowIcons()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private static void OnProjectWindowItemGUI(string guid, Rect rect)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);

        if (itemData == null || itemData.Icon == null)
        {
            return;
        }

        bool listMode = rect.width > rect.height * 2f;

        if (!listMode)
        {
            return;
        }

        Texture2D tex = AssetPreview.GetMiniThumbnail(itemData.Icon);

        if (tex == null)
        {
            tex = AssetPreview.GetAssetPreview(itemData.Icon);
        }

        if (tex == null)
        {
            return;
        }

        Rect iconRect = new(rect.x + 2f, rect.y + 1f, rect.height - 2f, rect.height - 2f);
        GUI.DrawTexture(iconRect, tex, ScaleMode.ScaleToFit, true);
    }
}
#endif