using System.Collections.Generic;
using UnityEngine;

public static class CrateLootGenerationService
{
    public static bool TryGenerate(
        List<InventorySlot> slots,
        IReadOnlyList<CrateLootEntry> lootTable,
        int maxGeneratedItemCount,
        float maxWeightKg)
    {
        if (slots == null || lootTable == null || lootTable.Count == 0 || maxGeneratedItemCount <= 0)
        {
            return false;
        }

        int targetCount = Random.Range(0, maxGeneratedItemCount + 1);

        if (targetCount <= 0)
        {
            return false;
        }

        int spawnedCount = 0;
        bool generatedAny = false;
        List<CrateLootEntry> candidates = new(lootTable.Count);

        for (int i = 0; i < lootTable.Count; i++)
        {
            candidates.Add(lootTable[i]);
        }

        Shuffle(candidates);

        for (int i = 0; i < candidates.Count && spawnedCount < targetCount; i++)
        {
            CrateLootEntry entry = candidates[i];

            if (!ShouldUseEntry(entry))
            {
                continue;
            }

            int remainingCount = targetCount - spawnedCount;
            int count = Mathf.Min(remainingCount, Random.Range(entry.MinCount, entry.MaxCount + 1));

            if (CrateInventoryAddService.TryAddItem(slots, maxWeightKg, entry.Item, count))
            {
                spawnedCount += count;
                generatedAny = true;
            }
        }

        return generatedAny;
    }

    private static bool ShouldUseEntry(CrateLootEntry entry)
    {
        return entry != null
            && entry.Item != null
            && Random.value <= entry.Chance;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
