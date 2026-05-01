using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CrateUIController
{
    private void EnsureRuntimeReferences()
    {
        if (_inventoryController == null)
        {
            _inventoryController = CrateSceneReferenceResolver.FindSceneObject<InventoryController>();
        }

        _playerInput = PlayerInputResolver.Resolve(_playerInput);
    }

    private void AutoWireSceneReferences()
    {
        if (_inventoryUIController == null)
        {
            _inventoryUIController = CrateSceneReferenceResolver.FindSceneObject<InventoryUIController>();
        }

        if (_interactController == null)
        {
            _interactController = CrateSceneReferenceResolver.FindSceneObject<InteractController>();
        }

        if (_inventoryUIController != null)
        {
            _crateRoot ??= _inventoryUIController.InventoryRoot;
            _playerGridRoot ??= _inventoryUIController.GridRoot;
            _cellPrefab ??= _inventoryUIController.CellPrefab;
            _searchProgressRoot ??= _inventoryUIController.UseProgressModalRoot;
            _searchProgressFillImage ??= _inventoryUIController.UseProgressFillImage;
            _searchProgressText ??= _inventoryUIController.UseProgressText;

            if (IsNullOrEmpty(_disableWhileOpen))
            {
                _disableWhileOpen = _inventoryUIController.DisableWhileOpen;
            }

            if (IsNullOrEmpty(_objectDisableWhileOpen))
            {
                _objectDisableWhileOpen = _inventoryUIController.ObjectDisableWhileOpen;
            }
        }

        if (_interactController != null)
        {
            _takeItemRoot ??= _interactController.InspectRoot;
            _takeItemIcon ??= _interactController.InspectIcon;
            _takeItemDurabilityIcon ??= _interactController.InspectDurabilityIcon;
            _takeItemNameText ??= _interactController.InspectNameText;
            _takeItemDescriptionText ??= _interactController.InspectDescriptionText;
            _takeItemDurabilityText ??= _interactController.InspectDurabilityText;
            _takeItemWeightText ??= _interactController.InspectWeightText;
        }

        _crateRoot ??= CrateSceneReferenceResolver.FindSceneGameObject("InventoryRoot");
        _rightCratePanel ??= CrateSceneReferenceResolver.FindSceneGameObject("Right Crate Panel");
        _inventoryRightPanel ??= CrateSceneReferenceResolver.FindSceneGameObject("Right Panel");

        if (_crateRoot == null && _rightCratePanel != null)
        {
            _crateRoot = _rightCratePanel;
        }

        if (_rightCratePanel != null)
        {
            _crateGridRoot ??= CrateSceneReferenceResolver.FindDeepChildByPath(_rightCratePanel.transform, "Scroll View", "Viewport", "Content")
                ?? CrateSceneReferenceResolver.FindDeepChild(_rightCratePanel.transform, "Content");
            _crateActionButton ??= CrateSceneReferenceResolver.FindComponentInChildrenByName<Button>(_rightCratePanel, "Crate Action Button");
            _crateWeightText ??= CrateSceneReferenceResolver.FindComponentInChildrenByName<TMP_Text>(_rightCratePanel, "Weight Text");
            _crateWeightSlider ??= CrateSceneReferenceResolver.FindComponentInChildrenByName<Slider>(_rightCratePanel, "Weight Slider");
        }
    }

    private static bool IsNullOrEmpty<T>(T[] array)
    {
        return array == null || array.Length == 0;
    }
}
