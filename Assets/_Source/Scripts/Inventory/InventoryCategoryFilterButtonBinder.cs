using System;
using System.Collections.Generic;

public static class InventoryCategoryFilterButtonBinder
{
    public static void Bind(
        CategoryFilterButton[] configs,
        Action<InventoryCategoryFilter> onClicked,
        List<InventoryButtonBinding> bindings)
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
            CategoryFilterButton config = configs[i];

            if (config.Button == null)
            {
                continue;
            }

            InventoryCategoryFilter filter = config.Filter;
            void action() => onClicked(filter);

            config.Button.onClick.AddListener(action);
            bindings.Add(new InventoryButtonBinding(config.Button, action));
        }
    }
}