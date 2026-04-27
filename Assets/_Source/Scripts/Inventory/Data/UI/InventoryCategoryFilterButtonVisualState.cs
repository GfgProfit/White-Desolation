using UnityEngine;

public readonly struct InventoryCategoryFilterButtonVisualState
{
    public Color RootColor { get; }
    public float IconAlpha { get; }

    public InventoryCategoryFilterButtonVisualState(Color rootColor, float iconAlpha)
    {
        RootColor = rootColor;
        IconAlpha = iconAlpha;
    }
}