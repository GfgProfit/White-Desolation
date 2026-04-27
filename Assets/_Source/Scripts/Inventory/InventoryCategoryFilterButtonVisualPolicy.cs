using UnityEngine;

public static class InventoryCategoryFilterButtonVisualPolicy
{
    public static InventoryCategoryFilterButtonVisualState Build(InventoryCategoryFilter buttonFilter, InventoryCategoryFilter activeFilter, Color selectedRootColor, Color unselectedRootColor, float selectedIconAlpha, float unselectedIconAlpha)
    {
        bool isSelected = buttonFilter == activeFilter;

        return new InventoryCategoryFilterButtonVisualState(isSelected ? selectedRootColor : unselectedRootColor, isSelected ? selectedIconAlpha : unselectedIconAlpha);
    }
}