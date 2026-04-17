using UnityEngine;

public class LegacyPlayerInput : IPlayerInput
{
    public Vector2 GetMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical);
    }

    public Vector2 GetMouseDelta()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new Vector2(mouseX, mouseY);
    }

    public bool IsJumpPressed() => Input.GetKeyDown(KeyCode.Space);
    public bool IsSprintHeld() => Input.GetKey(KeyCode.LeftShift);

    public bool IsInteractPressed() => Input.GetKeyDown(KeyCode.F);
    public bool IsInteractHold() => Input.GetKey(KeyCode.F);
    public bool IsInteractUp() => Input.GetKeyUp(KeyCode.F);

    public bool IsAimingHold() => Input.GetKey(KeyCode.Mouse1);
    public bool IsReloadingPressed() => Input.GetKeyDown(KeyCode.R);
    public bool IsShootingHold() => Input.GetKey(KeyCode.Mouse0);
    public bool IsShootingPressed() => Input.GetKeyDown(KeyCode.Mouse0);
    public bool IsPrimaryWeaponPressed() => Input.GetKeyDown(KeyCode.Alpha1);
    public bool IsSecondaryWeaponPressed() => Input.GetKeyDown(KeyCode.Alpha2);
    public bool IsCrouchingHold() => Input.GetKey(KeyCode.LeftControl);

    public bool IsEscapePressed() => Input.GetKeyDown(KeyCode.Escape);
    public bool IsInventoryPressed() => Input.GetKeyDown(KeyCode.Tab);
}