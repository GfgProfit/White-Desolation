using UnityEngine.Events;
using UnityEngine.UI;

public struct SortButtonBinding
{
    public Button Button;
    public UnityAction Action;

    public SortButtonBinding(Button button, UnityAction action)
    {
        Button = button;
        Action = action;
    }
}