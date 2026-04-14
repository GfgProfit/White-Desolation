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

    public bool IsJumpPressed()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    public bool IsSprintHeld()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    public bool IsInteractPressed()
    {
        return Input.GetKeyDown(KeyCode.F);
    }

    public bool IsInteractHold()
    {
        return Input.GetKey(KeyCode.F);
    }

    public bool IsInteractUp()
    {
        return Input.GetKeyUp(KeyCode.F);
    }

    public bool IsAimingHold()
    {
        return Input.GetKey(KeyCode.Mouse1);
    }

    public bool IsReloadingPressed()
    {
        return Input.GetKeyDown(KeyCode.R);
    }

    public bool IsShootingHold()
    {
        return Input.GetKey(KeyCode.Mouse0);
    }

    public bool IsShootingPressed()
    {
        return Input.GetKeyDown(KeyCode.Mouse0);
    }

    public bool IsPrimaryWeaponPressed()
    {
        return Input.GetKeyDown(KeyCode.Alpha1);
    }

    public bool IsSecondaryWeaponPressed()
    {
        return Input.GetKeyDown(KeyCode.Alpha2);
    }

    public bool IsCrouchingHold()
    {
        return Input.GetKey(KeyCode.LeftControl);
    }

    public bool IsEscapePressed()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }
}