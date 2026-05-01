using System.Collections.Generic;

public static class CraftingInventoryQuery
{
    public static int GetOwnedCount(InventoryController inventory, ItemData item)
    {
        return inventory != null && item != null ? inventory.GetTotalCount(item) : 0;
    }

    public static bool HasRequiredItems(InventoryController inventory, CraftRecipe recipe)
    {
        if (inventory == null || recipe == null)
        {
            return false;
        }

        IReadOnlyList<CraftRequirement> requirements = recipe.Requirements;

        if (requirements == null)
        {
            return true;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            CraftRequirement requirement = requirements[i];

            if (requirement == null || !requirement.IsValid)
            {
                continue;
            }

            if (HasPreviousRequirementForItem(requirements, requirement.Item, i))
            {
                continue;
            }

            int requiredCount = GetRequiredCount(requirements, requirement.Item);

            if (inventory.GetTotalCount(requirement.Item) < requiredCount)
            {
                return false;
            }
        }

        return true;
    }

    public static bool HasToolRequirement(CraftRecipe recipe)
    {
        IReadOnlyList<CraftToolRequirement> requirements = recipe != null ? recipe.ToolRequirements : null;

        if (requirements == null)
        {
            return false;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            CraftToolRequirement requirement = requirements[i];

            if (requirement != null && requirement.IsValid)
            {
                return true;
            }
        }

        return false;
    }

    public static void BuildToolCandidates(InventoryController inventory, CraftRecipe recipe, List<CraftToolCandidate> candidates)
    {
        candidates?.Clear();

        if (inventory == null || recipe == null || candidates == null)
        {
            return;
        }

        IReadOnlyList<CraftToolRequirement> requirements = recipe.ToolRequirements;

        if (requirements == null || requirements.Count == 0)
        {
            return;
        }

        IReadOnlyList<InventorySlot> slots = inventory.Items;

        if (slots == null)
        {
            return;
        }

        for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
        {
            InventorySlot slot = slots[slotIndex];

            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                continue;
            }

            if (slot.HasDurability && slot.IsBroken)
            {
                continue;
            }

            CraftToolRequirement matchingRequirement = FindMatchingToolRequirement(requirements, slot.Item);

            if (matchingRequirement == null)
            {
                continue;
            }

            candidates.Add(new CraftToolCandidate(slotIndex, slot, matchingRequirement));
        }
    }

    private static CraftToolRequirement FindMatchingToolRequirement(IReadOnlyList<CraftToolRequirement> requirements, ItemData item)
    {
        if (requirements == null || item == null)
        {
            return null;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            CraftToolRequirement requirement = requirements[i];

            if (requirement == null || !requirement.IsValid)
            {
                continue;
            }

            if (ItemDataComparer.AreSame(requirement.Tool, item))
            {
                return requirement;
            }
        }

        return null;
    }

    public static int GetRequiredCount(IReadOnlyList<CraftRequirement> requirements, ItemData item)
    {
        if (requirements == null || item == null)
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < requirements.Count; i++)
        {
            CraftRequirement requirement = requirements[i];

            if (requirement == null || !requirement.IsValid)
            {
                continue;
            }

            if (ItemDataComparer.AreSame(requirement.Item, item))
            {
                count += requirement.Count;
            }
        }

        return count;
    }

    public static bool HasPreviousRequirementForItem(IReadOnlyList<CraftRequirement> requirements, ItemData item, int currentIndex)
    {
        if (requirements == null || item == null)
        {
            return false;
        }

        for (int i = 0; i < currentIndex; i++)
        {
            CraftRequirement requirement = requirements[i];

            if (requirement != null && requirement.IsValid && ItemDataComparer.AreSame(requirement.Item, item))
            {
                return true;
            }
        }

        return false;
    }
}
