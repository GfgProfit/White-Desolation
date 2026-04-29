using System.Collections.Generic;

public sealed class FireBurningOperationList
{
    private readonly List<FireBurningOperationListEntry> _entries = new();
    private readonly List<int> _slotIndexes = new();

    public IReadOnlyList<FireBurningOperationListEntry> Entries => _entries;
    public int Count => _entries.Count;

    public void Clear()
    {
        _entries.Clear();
        _slotIndexes.Clear();
    }

    public void Add(FireBurningOperationListEntry entry, int slotIndex = -1)
    {
        _entries.Add(entry);
        _slotIndexes.Add(slotIndex);
    }

    public bool IsInteractable(int index)
    {
        return index >= 0 && index < _entries.Count && _entries[index].Interactable;
    }

    public bool TryGetSlotIndex(int entryIndex, out int slotIndex)
    {
        slotIndex = -1;

        if (entryIndex < 0 || entryIndex >= _slotIndexes.Count)
        {
            return false;
        }

        slotIndex = _slotIndexes[entryIndex];

        return slotIndex >= 0;
    }
}
