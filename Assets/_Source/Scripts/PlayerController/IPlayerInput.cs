using UnityEngine;

public interface IPlayerInput
{
    Vector2 GetMovementInput();
    Vector2 GetMouseDelta();

    bool IsJumpPressed();
    bool IsSprintHeld();

    bool IsInteractPressed();
    bool IsInteractHold();
    bool IsInteractUp();

    bool IsAimingHold();
    bool IsReloadingPressed();
    bool IsShootingHold();
    bool IsShootingPressed();
    bool IsPrimaryWeaponPressed();
    bool IsSecondaryWeaponPressed();
    bool IsCrouchingHold();

    bool IsEscapePressed();
    bool IsInventoryPressed();
}