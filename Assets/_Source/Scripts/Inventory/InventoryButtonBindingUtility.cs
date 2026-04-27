using System.Collections.Generic;

public static class InventoryButtonBindingUtility
{
    public static void ReleaseAll(List<InventoryButtonBinding> bindings)
    {
        if (bindings == null)
        {
            return;
        }

        for (int i = 0; i < bindings.Count; i++)
        {
            bindings[i].Release();
        }

        bindings.Clear();
    }
}