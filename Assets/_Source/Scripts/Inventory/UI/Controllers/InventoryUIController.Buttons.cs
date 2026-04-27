public partial class InventoryUIController
{
    private void InitializeMainButtons()
    {
        if (_useButton != null)
        {
            _useButton.onClick.AddListener(HandleUseClicked);
        }

        if (_dropOneButton != null)
        {
            _dropOneButton.onClick.AddListener(HandleDropOneClicked);
        }
    }

    private void CleanupMainButtons()
    {
        if (_useButton != null)
        {
            _useButton.onClick.RemoveListener(HandleUseClicked);
        }

        if (_dropOneButton != null)
        {
            _dropOneButton.onClick.RemoveListener(HandleDropOneClicked);
        }
    }

    private void InitializeCategoryFilters()
    {
        InitializeCategoryFilterButtons();
        RefreshCategoryFilterButtonVisuals();
    }

    private void InitializeCategoryFilterButtons()
    {
        InventoryCategoryFilterButtonBinder.Bind(_categoryFilterButtons, HandleCategoryFilterClicked,  _categoryFilterButtonBindings);
    }

    private void CleanupCategoryFilterButtons()
    {
        InventoryButtonBindingUtility.ReleaseAll(_categoryFilterButtonBindings);
    }

    private void HandleCategoryFilterClicked(InventoryCategoryFilter filter)
    {
        if (_activeFilter == filter)
        {
            RefreshCategoryFilterButtonVisuals();
            return;
        }

        _activeFilter = filter;
        RefreshView();
    }

    private void RefreshCategoryFilterButtonVisuals()
    {
        if (_categoryFilterButtons == null)
        {
            return;
        }

        for (int i = 0; i < _categoryFilterButtons.Length; i++)
        {
            CategoryFilterButton config = _categoryFilterButtons[i];

            InventoryCategoryFilterButtonVisualState visualState = InventoryCategoryFilterButtonVisualPolicy.Build(config.Filter, _activeFilter, SelectedCategoryButtonColor, UnselectedCategoryButtonColor, SelectedCategoryIconAlpha, UnselectedCategoryIconAlpha);

            ApplyCategoryFilterButtonVisualState(config, visualState);
        }
    }

    private void ApplyCategoryFilterButtonVisualState(CategoryFilterButton config, InventoryCategoryFilterButtonVisualState visualState)
    {
        if (config.RootImage != null)
        {
            config.RootImage.color = visualState.RootColor;
        }

        if (config.IconCanvasGroup != null)
        {
            config.IconCanvasGroup.alpha = visualState.IconAlpha;
        }
    }

    private void InitializeSorting()
    {
        InitializeSortButtons();

        _activeSortMode = _defaultSortMode;
        _activeSortDirection = _defaultSortDirection;

        RefreshSortButtonVisuals();
    }

    private void InitializeSortButtons()
    {
        InventorySortButtonBinder.Bind(_sortButtons, HandleSortButtonClicked, _sortButtonBindings);
    }

    private void CleanupSortButtons()
    {
        InventoryButtonBindingUtility.ReleaseAll(_sortButtonBindings);
    }

    private void HandleSortButtonClicked(InventorySortMode mode)
    {
        if (mode == InventorySortMode.None)
        {
            return;
        }

        if (_activeSortMode == mode)
        {
            _activeSortDirection = _activeSortDirection == InventorySortDirection.Ascending ? InventorySortDirection.Descending : InventorySortDirection.Ascending;
        }
        else
        {
            _activeSortMode = mode;
            _activeSortDirection = InventorySortDirection.Ascending;
        }

        RefreshView();
    }

    private void RefreshSortButtonVisuals()
    {
        if (_sortButtons == null)
        {
            return;
        }

        for (int i = 0; i < _sortButtons.Length; i++)
        {
            SortButtonConfig config = _sortButtons[i];

            InventorySortButtonVisualState visualState = InventorySortButtonVisualPolicy.Build(config.Mode, _activeSortMode, _selectedSortButtonAlpha, _unselectedSortButtonAlpha);

            ApplySortButtonVisualState(config, visualState);
        }
    }

    private void ApplySortButtonVisualState(SortButtonConfig config, InventorySortButtonVisualState visualState)
    {
        if (config.CanvasGroup != null)
        {
            config.CanvasGroup.alpha = visualState.Alpha;
        }
    }
}