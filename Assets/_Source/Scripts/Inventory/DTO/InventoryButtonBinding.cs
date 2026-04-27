using UnityEngine.Events;
using UnityEngine.UI;

public readonly struct InventoryButtonBinding
{
    public Button Button { get; }
    public UnityAction Action { get; }

    public InventoryButtonBinding(Button button, UnityAction action)
    {
        Button = button;
        Action = action;
    }

    public void Release()
    {
        if (Button != null && Action != null)
        {
            Button.onClick.RemoveListener(Action);
        }
    }
}