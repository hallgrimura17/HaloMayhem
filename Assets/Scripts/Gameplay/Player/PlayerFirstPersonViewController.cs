using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Controls the owner's fixed first-person camera, look rig, and world-avatar shadow presentation.
/// </summary>
public sealed class PlayerFirstPersonViewController : MonoBehaviour
{
    private const float MaximumPitch = 85f;
    private const float MouseLookSensitivity = 0.08f;
    private const float GamepadLookSpeed = 180f;
    private const float CrouchTransitionDuration = 0.18f;
    private const float RagdollCameraSmoothTime = 0.12f;

    [SerializeField] private Transform _playerRoot;
    [SerializeField] private Camera _camera;
    [SerializeField] private AudioListener _audioListener;
    [SerializeField] private GameObject _worldAvatar;
    [SerializeField] private float _standingCameraHeight = 1.62f;
    [SerializeField] private float _crouchingCameraHeight = 1.05f;
    [SerializeField] private Vector3 _ragdollFollowOffset = new Vector3(0f, 0.65f, -1.25f);

    private float _pitch;
    private float _crouchBlend;
    private Vector3 _ragdollCameraVelocity;
    private Vector3 _standingCameraLocalPosition;
    private Transform _ragdollHips;
    private Renderer[] _worldAvatarRenderers;
    private ShadowCastingMode[] _worldAvatarShadowModes;
    private bool _wasOwner;

    private void Awake()
    {
        _standingCameraLocalPosition = transform.localPosition;
        _worldAvatarRenderers = _worldAvatar.GetComponentsInChildren<Renderer>(true);
        _worldAvatarShadowModes = new ShadowCastingMode[_worldAvatarRenderers.Length];
        for (int i = 0; i < _worldAvatarRenderers.Length; i++)
            _worldAvatarShadowModes[i] = _worldAvatarRenderers[i].shadowCastingMode;
    }

    /// <summary>
    /// Enables first-person presentation for the owner and makes the avatar cast shadows without rendering locally.
    /// </summary>
    public void SetOwner(bool isOwner)
    {
        _camera.enabled = isOwner;
        _audioListener.enabled = isOwner;
        SetWorldAvatarShadowOnly(isOwner);

        if (isOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (_wasOwner)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        _wasOwner = isOwner;
    }

    /// <summary>
    /// Toggles the world avatar between normal rendering and shadow-only rendering.
    /// </summary>
    public void SetWorldAvatarShadowOnly(bool shadowOnly)
    {
        for (int i = 0; i < _worldAvatarRenderers.Length; i++)
            _worldAvatarRenderers[i].shadowCastingMode = shadowOnly ? ShadowCastingMode.ShadowsOnly : _worldAvatarShadowModes[i];
    }

    /// <summary>
    /// Starts or stops following the local ragdoll hips with the first-person camera.
    /// </summary>
    public void SetRagdollCameraTarget(Transform hips)
    {
        _ragdollHips = hips;
        _ragdollCameraVelocity = Vector3.zero;
        if (_ragdollHips == null)
        {
            transform.localPosition = _standingCameraLocalPosition;
            transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }

    /// <summary>
    /// Applies mouse- or gamepad-scaled first-person look input to the player yaw and camera pitch.
    /// </summary>
    public void ApplyLook(Vector2 lookInput, bool isMouse, float deltaTime)
    {
        float scale = isMouse ? MouseLookSensitivity : GamepadLookSpeed * deltaTime;
        _playerRoot.Rotate(0f, lookInput.x * scale, 0f, Space.World);
        _pitch = Mathf.Clamp(_pitch - lookInput.y * scale, -MaximumPitch, MaximumPitch);
        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    /// <summary>
    /// Smoothly moves the first-person camera mount to the standing or crouched eye height.
    /// </summary>
    public void SetCrouched(bool isCrouched, float deltaTime)
    {
        float targetBlend = isCrouched ? 1f : 0f;
        _crouchBlend = Mathf.MoveTowards(_crouchBlend, targetBlend, deltaTime / CrouchTransitionDuration);
        Vector3 localPosition = transform.localPosition;
        localPosition.y = Mathf.Lerp(_standingCameraHeight, _crouchingCameraHeight, _crouchBlend);
        transform.localPosition = localPosition;
    }

    private void LateUpdate()
    {
        if (_ragdollHips == null)
            return;

        Vector3 targetPosition = _ragdollHips.TransformPoint(_ragdollFollowOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _ragdollCameraVelocity, RagdollCameraSmoothTime);
        Vector3 lookDirection = _ragdollHips.position - transform.position;
        if (lookDirection.sqrMagnitude > 0f)
            transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }
}
