using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads the owning client's FPS movement actions and moves its CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public sealed class NetworkPlayerController : NetworkBehaviour
{
    private const string PlayerMapName = "Player";
    private const string MoveActionName = "Move";
    private const string LookActionName = "Look";
    private const string JumpActionName = "Jump";
    private const string CrouchActionName = "Crouch";
    private const string SprintActionName = "Sprint";
    private const string RagdollTestActionName = "RagdollTest";
    private const string MoveXParameter = "MoveX";
    private const string MoveYParameter = "MoveY";
    private const string IsCrouchingParameter = "IsCrouching";
    private const string IsSprintingParameter = "IsSprinting";
    private const string IsJumpingParameter = "IsJumping";
    private const float CrouchedHeight = 1.1f;
    private const float CrouchTransitionDuration = 0.18f;
    private const float GroundedVelocity = -2f;
    private const float Gravity = -25f;

    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private PlayerFirstPersonViewController _firstPersonView;
    [SerializeField] private CapsuleCollider _proxyCollider;
    [SerializeField] private NetworkPlayerRagdoll _ragdoll;
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 7f;
    [SerializeField] private float _crouchSpeed = 2.5f;
    [SerializeField] private float _jumpHeight = 1.25f;

    private CharacterController _characterController;
    private Animator _animator;
    private InputActionMap _playerActionMap;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _crouchAction;
    private InputAction _sprintAction;
    private InputAction _ragdollTestAction;
    private float _standingHeight;
    private float _verticalVelocity;
    private float _crouchBlend;
    private bool _ownsClient;
    private bool _isRagdolled;
    private bool _characterControllerWasEnabled;
    private bool _proxyColliderWasEnabled;

    public Vector3 CurrentMovementVelocity => _characterController.velocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _standingHeight = _characterController.height;
    }

    /// <summary>
    /// Enables proxy collision shapes on the server for players not owned by the local server client.
    /// </summary>
    public override void OnStartServer()
    {
        SetProxyColliderEnabled(!Owner.IsLocalClient);
    }

    /// <summary>
    /// Enables owner input and first-person presentation when the network object starts on a client.
    /// </summary>
    public override void OnStartClient()
    {
        SetProxyColliderEnabled(!IsOwner);
        _firstPersonView.SetOwner(IsOwner);
        if (!IsOwner)
            return;

        _ownsClient = true;
        _playerActionMap = _inputActions.FindActionMap(PlayerMapName, true);
        _moveAction = _playerActionMap.FindAction(MoveActionName, true);
        _lookAction = _playerActionMap.FindAction(LookActionName, true);
        _jumpAction = _playerActionMap.FindAction(JumpActionName, true);
        _crouchAction = _playerActionMap.FindAction(CrouchActionName, true);
        _sprintAction = _playerActionMap.FindAction(SprintActionName, true);
        _ragdollTestAction = _playerActionMap.FindAction(RagdollTestActionName, true);
        _playerActionMap.Enable();
    }

    /// <summary>
    /// Disables owner input and releases first-person presentation when the network object stops on a client.
    /// </summary>
    public override void OnStopClient()
    {
        if (!_ownsClient)
            return;

        _playerActionMap.Disable();
        _firstPersonView.SetOwner(false);
        _ownsClient = false;
    }

    private void Update()
    {
        if (!_ownsClient)
            return;

        if (_ragdollTestAction.WasPressedThisFrame())
        {
            _ragdoll.RequestRagdollToggle();
            return;
        }

        if (_isRagdolled)
            return;

        float deltaTime = Time.deltaTime;
        Vector2 lookInput = _lookAction.ReadValue<Vector2>();
        _firstPersonView.ApplyLook(lookInput, _lookAction.activeControl?.device is Mouse, deltaTime);

        Vector2 moveInput = _moveAction.ReadValue<Vector2>().normalized;
        bool crouched = _crouchAction.IsPressed();
        bool sprinting = _sprintAction.IsPressed() && !crouched && moveInput.sqrMagnitude > 0f;
        UpdateCrouch(crouched, deltaTime);
        _firstPersonView.SetCrouched(crouched, deltaTime);

        float speed = crouched ? _crouchSpeed : sprinting ? _sprintSpeed : _walkSpeed;
        Vector3 horizontalMovement = (transform.right * moveInput.x + transform.forward * moveInput.y) * speed;
        if (_characterController.isGrounded)
        {
            _verticalVelocity = GroundedVelocity;
            if (_jumpAction.WasPressedThisFrame())
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * Gravity);
        }
        else
        {
            _verticalVelocity += Gravity * deltaTime;
        }

        _characterController.Move((horizontalMovement + Vector3.up * _verticalVelocity) * deltaTime);
        _animator.SetFloat(MoveXParameter, moveInput.x);
        _animator.SetFloat(MoveYParameter, moveInput.y);
        _animator.SetBool(IsCrouchingParameter, crouched);
        _animator.SetBool(IsSprintingParameter, sprinting);
        _animator.SetBool(IsJumpingParameter, !_characterController.isGrounded);
    }

    /// <summary>
    /// Enables or disables ragdoll mode and restores the previous player collider states when recovering.
    /// </summary>
    public void SetRagdolled(bool ragdolled)
    {
        if (_isRagdolled == ragdolled)
            return;

        _isRagdolled = ragdolled;
        if (ragdolled)
        {
            _characterControllerWasEnabled = _characterController.enabled;
            _proxyColliderWasEnabled = _proxyCollider.enabled;
            _characterController.enabled = false;
            _proxyCollider.enabled = false;
            return;
        }

        _characterController.enabled = _characterControllerWasEnabled;
        _proxyCollider.enabled = _proxyColliderWasEnabled;
    }

    private void LateUpdate()
    {
        if (_ownsClient || !_proxyCollider.enabled)
            return;

        float proxyHeight = _animator.GetBool(IsCrouchingParameter) ? CrouchedHeight : _standingHeight;
        _proxyCollider.height = proxyHeight;
        _proxyCollider.transform.localPosition = Vector3.up * (proxyHeight * 0.5f);
    }

    private void SetProxyColliderEnabled(bool enabled)
    {
        _proxyCollider.enabled = enabled;
    }

    private void UpdateCrouch(bool crouched, float deltaTime)
    {
        float targetBlend = crouched ? 1f : 0f;
        _crouchBlend = Mathf.MoveTowards(_crouchBlend, targetBlend, deltaTime / CrouchTransitionDuration);
        _characterController.height = Mathf.Lerp(_standingHeight, CrouchedHeight, _crouchBlend);
        _characterController.center = Vector3.up * (_characterController.height * 0.5f);
    }
}
