using System.Collections.Generic;

public sealed class CraftingService
{
    private readonly InventoryController _inventory;

    public CraftingService(InventoryController inventory)
    {
        _inventory = inventory;
    }

    public bool CanCraft(CraftRecipe recipe, CraftToolCandidate? selectedTool)
    {
        if (_inventory == null || recipe == null || !recipe.IsValid)
        {
            return false;
        }

        if (!_inventory.CanAddItem(recipe.ResultItem, recipe.ResultCount))
        {
            return false;
        }

        if (!CraftingInventoryQuery.HasRequiredItems(_inventory, recipe))
        {
            return false;
        }

        if (!CraftingInventoryQuery.HasToolRequirement(recipe))
        {
            return true;
        }

        return IsSelectedToolValid(selectedTool);
    }

    public bool CompleteCraft(CraftRecipe recipe, CraftToolCandidate? selectedTool)
    {
        if (!CanCraft(recipe, selectedTool))
        {
            return false;
        }

        if (selectedTool.HasValue)
        {
            CraftToolCandidate tool = selectedTool.Value;

            if (!_inventory.TryConsumeDurabilityFromSlot(tool.SlotIndex, tool.Tool, tool.DurabilityCost))
            {
                return false;
            }
        }

        IReadOnlyList<CraftRequirement> requirements = recipe.Requirements;

        if (requirements != null)
        {
            for (int i = 0; i < requirements.Count; i++)
            {
                CraftRequirement requirement = requirements[i];

                if (requirement == null || !requirement.IsValid)
                {
                    continue;
                }

                if (!_inventory.TryRemoveItem(requirement.Item, requirement.Count))
                {
                    return false;
                }
            }
        }

        return _inventory.TryAddItem(recipe.ResultItem, recipe.ResultCount);
    }

    private static bool IsSelectedToolValid(CraftToolCandidate? selectedTool)
    {
        if (!selectedTool.HasValue)
        {
            return false;
        }

        CraftToolCandidate tool = selectedTool.Value;

        if (tool.Slot == null || tool.Slot.IsEmpty || tool.Tool == null)
        {
            return false;
        }

        if (tool.Requirement == null || !tool.Requirement.IsValid)
        {
            return false;
        }

        if (!ItemDataComparer.AreSame(tool.Requirement.Tool, tool.Tool))
        {
            return false;
        }

        return !tool.Slot.HasDurability || !tool.Slot.IsBroken;
    }
}
