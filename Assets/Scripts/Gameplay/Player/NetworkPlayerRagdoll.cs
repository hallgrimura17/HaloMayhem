using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

/// <summary>
/// Activates or recovers the ragdoll using only the rigidbodies, joints, and colliders authored on the skeleton.
/// </summary>
public sealed class NetworkPlayerRagdoll : NetworkBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private NetworkPlayerController _playerController;
    [SerializeField] private PlayerFirstPersonViewController _firstPersonView;
    [SerializeField] private Transform _hips;

    private readonly List<Rigidbody> _rigidbodies = new();
    private readonly List<CapsuleCollider> _hitColliders = new();
    private readonly List<BonePose> _bonePoses = new();
    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private bool _updateWhenOffscreenBeforeRagdoll;
    private bool _serverRagdollActive;
    private bool _localRagdollActive;

    private void Awake()
    {
        _skinnedMeshRenderer = _animator.GetComponentInChildren<SkinnedMeshRenderer>(true);
        _updateWhenOffscreenBeforeRagdoll = _skinnedMeshRenderer.updateWhenOffscreen;
        _rigidbodies.AddRange(_animator.GetComponentsInChildren<Rigidbody>(true));
        _hitColliders.AddRange(_animator.GetComponentsInChildren<CapsuleCollider>(true));

        foreach (Rigidbody body in _rigidbodies)
        {
            _bonePoses.Add(new BonePose(body.transform, body.transform.localPosition, body.transform.localRotation));
            body.useGravity = false;
            body.isKinematic = true;
        }

        foreach (CapsuleCollider hitCollider in _hitColliders)
            hitCollider.isTrigger = true;
    }

    /// <summary>
    /// Requests the server to toggle this player's ragdoll state.
    /// </summary>
    public void RequestRagdollToggle()
    {
        ToggleRagdollServerRpc(_playerController.CurrentMovementVelocity);
    }

    /// <summary>
    /// Toggles ragdoll physics on the server for gameplay systems and developer testing.
    /// </summary>
    [Server]
    public void ToggleRagdollOnServer()
    {
        SetRagdollOnServer(!_serverRagdollActive, _playerController.CurrentMovementVelocity);
    }

    /// <summary>
    /// Forces ragdoll physics on from server-authoritative gameplay such as projectile damage.
    /// </summary>
    [Server]
    public void RagdollOnServer()
    {
        SetRagdollOnServer(true, _playerController.CurrentMovementVelocity);
    }

    /// <summary>
    /// Returns this player to animated movement from server-authoritative gameplay.
    /// </summary>
    [Server]
    public void RecoverOnServer()
    {
        SetRagdollOnServer(false, Vector3.zero);
    }

    [ServerRpc]
    private void ToggleRagdollServerRpc(Vector3 currentMovementVelocity)
    {
        SetRagdollOnServer(!_serverRagdollActive, currentMovementVelocity);
    }

    [ObserversRpc(BufferLast = true, RunLocally = true)]
    private void SetRagdollObserversRpc(bool ragdolled, Vector3 movementVelocity)
    {
        if (_localRagdollActive == ragdolled)
            return;

        _localRagdollActive = ragdolled;
        if (ragdolled)
            ActivateLocalRagdoll(movementVelocity);
        else
            RecoverLocalRagdoll();
    }

    private void SetRagdollOnServer(bool ragdolled, Vector3 movementVelocity)
    {
        if (_serverRagdollActive == ragdolled)
            return;

        _serverRagdollActive = ragdolled;
        SetRagdollObserversRpc(ragdolled, ragdolled ? movementVelocity : Vector3.zero);
    }

    private void ActivateLocalRagdoll(Vector3 movementVelocity)
    {
        _playerController.SetRagdolled(true);
        if (IsOwner)
            _firstPersonView.SetRagdollCameraTarget(_hips);
        _firstPersonView.SetWorldAvatarShadowOnly(false);
        _skinnedMeshRenderer.updateWhenOffscreen = true;
        _animator.enabled = false;
        for (int i = 0; i < _hitColliders.Count; i++)
            _hitColliders[i].isTrigger = false;
        for (int i = 0; i < _rigidbodies.Count; i++)
        {
            _rigidbodies[i].useGravity = true;
            _rigidbodies[i].isKinematic = false;
        }
        _hips.GetComponent<Rigidbody>().linearVelocity = movementVelocity;
    }

    private void RecoverLocalRagdoll()
    {
        for (int i = 0; i < _rigidbodies.Count; i++)
        {
            _rigidbodies[i].isKinematic = true;
            _rigidbodies[i].useGravity = false;
        }
        for (int i = 0; i < _bonePoses.Count; i++)
        {
            _bonePoses[i].Bone.localPosition = _bonePoses[i].LocalPosition;
            _bonePoses[i].Bone.localRotation = _bonePoses[i].LocalRotation;
        }
        for (int i = 0; i < _hitColliders.Count; i++)
            _hitColliders[i].isTrigger = true;

        _animator.enabled = true;
        _skinnedMeshRenderer.updateWhenOffscreen = _updateWhenOffscreenBeforeRagdoll;
        if (IsOwner)
            _firstPersonView.SetRagdollCameraTarget(null);
        _firstPersonView.SetWorldAvatarShadowOnly(IsOwner);
        _playerController.SetRagdolled(false);
    }

    private readonly struct BonePose
    {
        public BonePose(Transform bone, Vector3 localPosition, Quaternion localRotation)
        {
            Bone = bone;
            LocalPosition = localPosition;
            LocalRotation = localRotation;
        }

        public Transform Bone { get; }
        public Vector3 LocalPosition { get; }
        public Quaternion LocalRotation { get; }
    }
}
