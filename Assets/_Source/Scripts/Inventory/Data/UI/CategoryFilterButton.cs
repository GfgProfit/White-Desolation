using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct CategoryFilterButton
{
    public InventoryCategoryFilter Filter;
    public Button Button;
    public Image RootImage;
    public CanvasGroup IconCanvasGroup;
}