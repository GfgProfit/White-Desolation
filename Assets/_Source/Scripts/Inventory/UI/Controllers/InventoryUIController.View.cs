using TMPro;
using UnityEngine;

public partial class InventoryUIController
{
    private void RefreshView()
    {
        if (_isGridExternallyOwned)
        {
            RefreshCarryWeight();
            RefreshCategoryFilterButtonVisuals();
            RefreshSortButtonVisuals();
            NotifyExternalGridRefreshRequested();
            return;
        }

        if (_inventoryController == null)
        {
            return;
        }

        RefreshCarryWeight();

        int preferredVisibleIndex = _selectionState.GetVisibleIndex(_visibleEntries);
        InventorySlot preferredSlot = _selectionState.GetSelectedSlot(_visibleEntries);

        RebuildVisibleEntries();
        _selectionState.ValidateForVisibleEntries(_visibleEntries, preferredVisibleIndex, preferredSlot);

        RebuildGrid();
        RefreshDetails();

        RefreshCategoryFilterButtonVisuals();
        RefreshSortButtonVisuals();
    }

    private void RefreshCarryWeight()
    {
        if (_carryWeightText == null || _inventoryController == null)
        {
            return;
        }

        InventoryCarryWeightViewModel viewModel = InventoryCarryWeightPresenter.Build(_inventoryController);
        ApplyCarryWeightViewModel(viewModel);
    }

    private void ApplyCarryWeightViewModel(InventoryCarryWeightViewModel viewModel)
    {
        if (_carryWeightText != null)
        {
            _carryWeightText.text = viewModel.CarryWeightText;
        }

        if (_currentWeightText != null)
        {
            _currentWeightText.text = viewModel.CurrentWeightText;
        }

        if (_carryWeightSlider != null)
        {
            _carryWeightSlider.maxValue = viewModel.SliderMaxValue;
            _carryWeightSlider.value = viewModel.SliderValue;
        }
    }

    private void RebuildVisibleEntries()
    {
        InventoryViewQuery.BuildVisibleEntries(_inventoryController, _activeFilter, _activeSortMode, _activeSortDirection, _visibleEntries);
    }

    private void RebuildGrid()
    {
        InventoryGridRenderer.Rebuild(_cellPrefab, _gridRoot, _spawnedCells, _visibleEntries, _selectionState, HandleSlotSelected);
    }

    private void HandleSlotSelected(int slotIndex)
    {
        if (_useRoutineState.IsUsingItem)
        {
            return;
        }

        _selectionState.SelectSlot(slotIndex);

        InventoryGridRenderer.RefreshSelection(_spawnedCells, _visibleEntries, _selectionState);

        RefreshDetails();
    }

    private void RefreshDetails()
    {
        int selectedSlotIndex = _selectionState.SelectedSlotIndex;

        InventorySlot slot = _inventoryController != null ? _inventoryController.GetSlotAt(selectedSlotIndex) : null;

        bool canDrop = _itemDropper != null && _itemDropper.CanDrop(slot) && !_useRoutineState.IsUsingItem;
        InventoryItemDetailsViewModel viewModel = InventoryItemDetailsPresenter.Build(slot, BuildItemUseContext(), canDrop);

        ApplyDetailsViewModel(viewModel);
    }

    private void ApplyDetailsViewModel(InventoryItemDetailsViewModel viewModel)
    {
        if (_useButton != null)
        {
            _useButton.interactable = viewModel.CanUse;
        }

        if (_dropOneButton != null)
        {
            _dropOneButton.interactable = viewModel.CanDrop;
        }

        if (_useButtonLabel != null)
        {
            _useButtonLabel.text = viewModel.PrimaryActionLabel;
        }

        if (_itemIcon != null)
        {
            _itemIcon.enabled = viewModel.IconEnabled;
            _itemIcon.sprite = viewModel.Icon;
        }

        if (_itemNameText != null)
        {
            _itemNameText.text = viewModel.NameText;
        }

        if (_itemDescriptionText != null)
        {
            _itemDescriptionText.text = viewModel.DescriptionText;
        }

        if (_itemCountText != null)
        {
            _itemCountText.text = viewModel.CountText;
        }

        SetStatRow(_durabilityHolder, _durabilityText, viewModel.Durability.IsVisible, viewModel.Durability.Text);

        if (viewModel.HasSelection)
        {
            Utils.SetDurabilityColor01(viewModel.SourceSlot.Durability01, _durabilityText, _durabilityIcon);
        }

        SetStatRow(_weightHolder, _weightText, viewModel.Weight.IsVisible, viewModel.Weight.Text);
        SetStatRow(_caloriesHolder, _caloriesText, viewModel.Calories.IsVisible, viewModel.Calories.Text);
        SetStatRow(_hydrationHolder, _hydrationText, viewModel.Hydration.IsVisible, viewModel.Hydration.Text);
    }

    private void SetStatRow(GameObject holder, TMP_Text textComponent, bool visible, string value)
    {
        if (holder != null)
        {
            holder.SetActive(visible);
        }

        if (textComponent != null)
        {
            textComponent.text = visible ? value : string.Empty;
        }
    }
}
