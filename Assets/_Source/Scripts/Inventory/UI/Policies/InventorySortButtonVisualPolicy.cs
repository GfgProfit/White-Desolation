public static class InventorySortButtonVisualPolicy
{
    public static InventorySortButtonVisualState Build(InventorySortMode buttonMode, InventorySortMode activeMode, float selectedAlpha, float unselectedAlpha)
    {
        bool isSelected = activeMode != InventorySortMode.None && buttonMode == activeMode;

        return new InventorySortButtonVisualState( isSelected ? selectedAlpha : unselectedAlpha);
    }
}