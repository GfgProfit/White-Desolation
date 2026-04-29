using UnityEngine;

public partial class PlayerController
{
    private void SetRawInput()
    {
        Vector2 input = _playerInput.GetMovementInput();
        _rawInput = new Vector3(input.x, _rawInput.y, input.y);
    }

    private void Move()
    {
        float speed = CalculateTargetSpeed();

        Vector3 move = (transform.right * _rawInput.x + transform.forward * _rawInput.z).normalized * speed;

        _characterController.Move(move * Time.deltaTime);
    }

    private float CalculateTargetSpeed()
    {
        float targetSpeed;

        if (IsCrouching)
        {
            targetSpeed = _crouchSpeed;
        }
        else
        {
            targetSpeed = IsSprinting ? _sprintSpeed : _walkSpeed;
        }

        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _fieldOfViewSmooth * Time.deltaTime);

        return _currentSpeed;
    }

    private void UpdateMotionState()
    {
        IsWalking = _rawInput.x != 0f || _rawInput.z != 0f;

        IsSprinting = CanSprinting && IsWalking && _playerInput.IsSprintHeld() && _rawInput.z > 0;
    }
}