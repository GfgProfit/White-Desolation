using UnityEngine;

public sealed class SaveHotkeyInputService
{
    public bool IsSavePressed()
    {
        return Input.GetKeyDown(KeyCode.F5);
    }

    public bool IsLoadPressed()
    {
        return Input.GetKeyDown(KeyCode.F6);
    }
}