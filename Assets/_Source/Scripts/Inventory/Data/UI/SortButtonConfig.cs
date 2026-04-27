using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct SortButtonConfig
{
    public InventorySortMode Mode;
    public Button Button;
    public CanvasGroup CanvasGroup;
}