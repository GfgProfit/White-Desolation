using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private float _walkSpeed = 3.0f;
    [SerializeField] private float _sprintSpeed = 6.0f;

    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravity = 9.81f;

    [SerializeField] private Vector2 _cameraClampLimit = new(-90.0f, 90.0f);

    [Space]
    [SerializeField] private float _defaultCameraFieldOfView = 60.0f;
    [SerializeField] private float _sprintCameraFieldOfView = 70.0f;
    [SerializeField] private float _fieldOfViewSmooth = 5.0f;

    [Space]
    [SerializeField] private float _mouseSensitivity = 2.0f;

    [Header("Crouch")]
    [SerializeField] private float _crouchHeight = 1.0f;
    [SerializeField] private float _standHeight = 2.0f;
    [SerializeField] private float _crouchSpeed = 1.5f;
    [SerializeField] private float _crouchTransitionSpeed = 6.0f;
    [SerializeField] private Vector3 _crouchCameraOffset = new(0, -0.5f, 0);

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private VignetteController _vignetteController;
    #endregion

    #region Private Fields
    [Inject] private IPlayerInput _playerInput;

    private float _xRotation;
    private float _currentSpeed;
    private Vector3 _rawInput;
    private float _verticalVelocity;
    private IPlayerInput _input;
    private Vector3 _cameraTargetLocalPos;
    private Vector3 _cameraDefaultLocalPos;
    #endregion

    #region Properties
    public bool IsSprinting { get; private set; }
    public bool CanSprinting { get; private set; } = true;
    public bool IsWalking { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool CanCrouching { get; private set; } = true;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _input = new LegacyPlayerInput();

        _currentSpeed = _walkSpeed;

        _cameraDefaultLocalPos = _cameraTransform.localPosition;
        _cameraTargetLocalPos = _cameraDefaultLocalPos;
    }

    private void OnValidate()
    {
        _characterController = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _cameraTransform = _mainCamera.transform;
    }

    private void Update()
    {
        Look();
        SetRawInput();
        UpdateMotionState();
        HandleCrouch();
        Move();
        UpdateCameraFieldOfView();
    }
    #endregion

    #region Core Logic
    private void Look()
    {
        Vector2 mouseDelta = _input.GetMouseDelta();
        float mouseX = mouseDelta.x * _mouseSensitivity;
        float mouseY = mouseDelta.y * _mouseSensitivity;

        _xRotation = Mathf.Clamp(_xRotation - mouseY, _cameraClampLimit.x, _cameraClampLimit.y);

        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void SetRawInput()
    {
        Vector2 input = _input.GetMovementInput();
        _rawInput = new Vector3(input.x, _rawInput.y, input.y);
    }

    private void Move()
    {
        float speed = CalculateTargetSpeed();

        if (_characterController.isGrounded)
        {
            if (_verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            if (_input.IsJumpPressed())
            {
                _verticalVelocity = _jumpForce;
            }
        }

        _verticalVelocity -= _gravity * Time.deltaTime;

        Vector3 move = (transform.right * _rawInput.x + transform.forward * _rawInput.z).normalized * speed;
        move.y = _verticalVelocity;

        _characterController.Move(move * Time.deltaTime);
    }

    private float CalculateTargetSpeed()
    {
        float targetSpeed;

        if (IsCrouching)
            targetSpeed = _crouchSpeed;
        else
            targetSpeed = IsSprinting ? _sprintSpeed : _walkSpeed;

        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _fieldOfViewSmooth * Time.deltaTime);

        return _currentSpeed;
    }

    private void UpdateCameraFieldOfView()
    {
        float targetFOV = IsSprinting && !IsCrouching ? _sprintCameraFieldOfView : _defaultCameraFieldOfView;
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, _fieldOfViewSmooth * Time.deltaTime);
    }

    private void UpdateMotionState()
    {
        IsWalking = _rawInput.x != 0f || _rawInput.z != 0f;

        IsSprinting = CanSprinting && IsWalking && _input.IsSprintHeld() && _rawInput.z > 0;
    }

    private void HandleCrouch()
    {
        if (!CanCrouching)
        {
            return;
        }

        bool crouchPressed = _playerInput.IsCrouchingHold();

        if (crouchPressed && !IsCrouching)
        {
            StartCrouch();
        }
        else if (!crouchPressed && IsCrouching)
        {
            StopCrouch();
        }

        _cameraTransform.localPosition = Vector3.Lerp(
            _cameraTransform.localPosition,
            _cameraTargetLocalPos,
            Time.deltaTime * _crouchTransitionSpeed);
    }

    private void StartCrouch()
    {
        IsCrouching = true;
        _characterController.height = _crouchHeight;
        _characterController.center = new Vector3(0, _crouchHeight / 2f, 0);
        _cameraTargetLocalPos = _cameraDefaultLocalPos + _crouchCameraOffset;

        _vignetteController.AnimateIntensityCrouch();
    }

    private void StopCrouch()
    {
        IsCrouching = false;
        _characterController.height = _standHeight;
        _characterController.center = new Vector3(0, _standHeight / 2f, 0);
        _cameraTargetLocalPos = _cameraDefaultLocalPos;

        _vignetteController.AnimateIntensityBase();
    }

    public void SetCanCrouching(bool canCrouching) => CanCrouching = canCrouching;
    public bool IsGrounded => _characterController.isGrounded;
    #endregion
}