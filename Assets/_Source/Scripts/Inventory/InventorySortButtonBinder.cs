using System;
using System.Collections.Generic;

public static class InventorySortButtonBinder
{
    public static void Bind(SortButtonConfig[] configs, Action<InventorySortMode> onClicked, List<InventoryButtonBinding> bindings)
    {
        if (bindings == null)
        {
            return;
        }

        InventoryButtonBindingUtility.ReleaseAll(bindings);

        if (configs == null || onClicked == null)
        {
            return;
        }

        for (int i = 0; i < configs.Length; i++)
        {
            SortButtonConfig config = configs[i];

            if (config.Button == null)
            {
                continue;
            }

            InventorySortMode mode = config.Mode;
            void action() => onClicked(mode);

            config.Button.onClick.AddListener(action);
            bindings.Add(new InventoryButtonBinding(config.Button, action));
        }
    }
}