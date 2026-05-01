using System.Collections.Generic;

public static class CrateSearchResultQuery
{
    public static void BuildAvailableSlots(CrateContainer crate, List<InventorySlot> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear();

        if (crate == null)
        {
            return;
        }

        for (int i = 0; i < crate.Items.Count; i++)
        {
            InventorySlot slot = crate.Items[i];

            if (slot != null && !slot.IsEmpty && slot.Item != null)
            {
                results.Add(slot);
            }
        }
    }
}
