#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ItemDataProjectIcons
{
    private struct CachedIcon
    {
        public bool Initialized;
        public bool IsValid;
        public Texture2D Texture;
        public Rect Uv;
    }

    private static readonly Dictionary<string, CachedIcon> _cache = new(256);

    static ItemDataProjectIcons()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        EditorApplication.projectChanged += OnProjectChanged;
    }

    private static void OnProjectChanged()
    {
        _cache.Clear();
        EditorApplication.RepaintProjectWindow();
    }

    private static void OnProjectWindowItemGUI(string guid, Rect rect)
    {
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }

        if (rect.width <= rect.height * 2f)
        {
            return;
        }

        if (!_cache.TryGetValue(guid, out CachedIcon cached))
        {
            cached = BuildCacheEntry(guid);
            _cache[guid] = cached;
        }

        if (!cached.Initialized || !cached.IsValid || cached.Texture == null)
        {
            return;
        }

        float iconSize = Mathf.Max(16f, rect.height - 2f);
        Rect iconRect = new(rect.x + 2f, rect.y + 1f, iconSize, iconSize);
        iconRect = Shrink(iconRect, 1f);

        EditorGUI.DrawRect(iconRect, new(0f, 0f, 0f, 0.12f));
        GUI.DrawTextureWithTexCoords(iconRect, cached.Texture, cached.Uv, true);
    }

    private static CachedIcon BuildCacheEntry(string guid)
    {
        CachedIcon result = new()
        {
            Initialized = true,
            IsValid = false,
            Texture = null,
            Uv = default
        };

        string path = AssetDatabase.GUIDToAssetPath(guid);

        if (string.IsNullOrEmpty(path) || !path.EndsWith(".asset"))
        {
            return result;
        }

        ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);

        if (itemData == null || itemData.Icon == null || itemData.Icon.texture == null)
        {
            return result;
        }

        Sprite sprite = itemData.Icon;
        Texture2D texture = sprite.texture;
        Rect sr = sprite.rect;

        result.IsValid = true;
        result.Texture = texture;
        result.Uv = new Rect(sr.x / texture.width, sr.y / texture.height, sr.width / texture.width, sr.height / texture.height);

        return result;
    }

    private static Rect Shrink(Rect rect, float padding)
    {
        return new Rect(rect.x + padding, rect.y + padding, Mathf.Max(1f, rect.width - padding * 2f), Mathf.Max(1f, rect.height - padding * 2f));
    }
}
#endif