using System;
using System.Collections.Generic;
using UnityEngine;

public static class InventoryGridRenderer
{
    public static void Rebuild(InventoryItemCellView cellPrefab, Transform gridRoot, List<InventoryItemCellView> spawnedCells, List<InventoryViewEntry> visibleEntries, InventorySelectionState selectionState, Action<int> onSlotSelected)
    {
        Clear(spawnedCells);

        if (cellPrefab == null || gridRoot == null || spawnedCells == null || visibleEntries == null)
        {
            return;
        }

        for (int i = 0; i < visibleEntries.Count; i++)
        {
            InventoryViewEntry entry = visibleEntries[i];

            InventoryItemCellView cell = UnityEngine.Object.Instantiate(cellPrefab, gridRoot);

            bool isSelected = selectionState != null
                && selectionState.IsSelected(entry.SlotIndex);

            cell.Bind(entry.Slot, entry.SlotIndex, isSelected, onSlotSelected);

            spawnedCells.Add(cell);
        }
    }

    public static void RefreshSelection(List<InventoryItemCellView> spawnedCells, List<InventoryViewEntry> visibleEntries, InventorySelectionState selectionState)
    {
        if (spawnedCells == null || visibleEntries == null || selectionState == null)
        {
            return;
        }

        for (int i = 0; i < spawnedCells.Count; i++)
        {
            if (spawnedCells[i] == null)
            {
                continue;
            }

            bool isSelected = i < visibleEntries.Count && selectionState.IsSelected(visibleEntries[i].SlotIndex);

            spawnedCells[i].SetSelected(isSelected);
        }
    }

    public static void Clear(List<InventoryItemCellView> spawnedCells)
    {
        if (spawnedCells == null)
        {
            return;
        }

        for (int i = 0; i < spawnedCells.Count; i++)
        {
            if (spawnedCells[i] != null)
            {
                UnityEngine.Object.Destroy(spawnedCells[i].gameObject);
            }
        }

        spawnedCells.Clear();
    }
}