using FishNet.Connection;
using FishNet.Object;

/// <summary>
/// Identifies the authenticated FishNet connection that owns this reusable network player.
/// </summary>
public sealed class PlayerIdentity : NetworkBehaviour
{
    /// <summary>
    /// Gets the FishNet connection assigned as owner of this network object.
    /// </summary>
    public NetworkConnection OwnerConnection => Owner;

    /// <summary>
    /// Gets the client identifier of the owning connection.
    /// </summary>
    public int OwnerClientId => Owner.ClientId;
}
