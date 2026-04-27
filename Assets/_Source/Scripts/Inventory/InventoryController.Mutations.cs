public partial class InventoryController
{
    public void ClearInventory()
    {
        _items.Clear();
        NotifyChanged();
    }

    private bool FinishInventoryMutation(bool mutated)
    {
        if (mutated)
        {
            NotifyChanged();
        }

        return mutated;
    }

    private void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}