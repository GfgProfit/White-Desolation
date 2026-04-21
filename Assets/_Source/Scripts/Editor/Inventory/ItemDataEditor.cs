#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        ItemData itemData = (ItemData)target;

        if (itemData == null || itemData.Icon == null || itemData.Icon.texture == null)
        {
            return null;
        }

        return BuildPreviewFromSprite(itemData.Icon, width, height);
    }

    private static Texture2D BuildPreviewFromSprite(Sprite sprite, int width, int height)
    {
        Texture2D source = sprite.texture;

        if (source == null || !source.isReadable)
        {
            return null;
        }

        Rect r = sprite.rect;

        Texture2D cropped = new((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
        cropped.SetPixels(source.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height));
        cropped.Apply();

        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture prev = RenderTexture.active;

        Graphics.Blit(cropped, rt);
        RenderTexture.active = rt;

        Texture2D result = new(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new(0, 0, width, height), 0, 0);
        result.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        DestroyImmediate(cropped);

        return result;
    }
}
#endif