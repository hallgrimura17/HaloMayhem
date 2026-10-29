using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;

/// <summary>
/// Bridges session requests to FishNet and exposes server-side player connection lifecycle events.
/// </summary>
public sealed class NetworkSessionCoordinator : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;

    private NetworkConnection _localConnection;
    private bool _hasLocalConnection;
    private readonly HashSet<int> _registeredConnectionIds = new();

    /// <summary>
    /// Raised for each server-side local or remote client connection that becomes active.
    /// </summary>
    public event Action<NetworkConnection> PlayerConnectionStarted;

    /// <summary>
    /// Raised when a server-side local or remote client connection stops.
    /// </summary>
    public event Action<NetworkConnection> PlayerConnectionStopped;

    private void OnEnable()
    {
        _networkManager.ServerManager.OnRemoteConnectionState += HandleRemoteConnectionState;
        _networkManager.ServerManager.OnAuthenticationResult += HandleServerAuthenticationResult;
        _networkManager.ClientManager.OnAuthenticated += HandleLocalClientAuthenticated;
        _networkManager.ClientManager.OnClientConnectionState += HandleLocalClientConnectionState;
    }

    private void OnDisable()
    {
        _networkManager.ServerManager.OnRemoteConnectionState -= HandleRemoteConnectionState;
        _networkManager.ServerManager.OnAuthenticationResult -= HandleServerAuthenticationResult;
        _networkManager.ClientManager.OnAuthenticated -= HandleLocalClientAuthenticated;
        _networkManager.ClientManager.OnClientConnectionState -= HandleLocalClientConnectionState;
    }

    /// <summary>
    /// Starts FishNet server and local client connections for a host session.
    /// </summary>
    public bool StartHost()
    {
        if (!_networkManager.ServerManager.StartConnection())
            return false;

        return _networkManager.ClientManager.StartConnection();
    }

    /// <summary>
    /// Starts a FishNet client connection using the transport's configured address.
    /// </summary>
    public bool StartClient()
    {
        return _networkManager.ClientManager.StartConnection();
    }

    /// <summary>
    /// Starts a FishNet client connection to the supplied transport address.
    /// </summary>
    public bool StartClient(string serverAddress)
    {
        return _networkManager.ClientManager.StartConnection(serverAddress);
    }

    /// <summary>
    /// Stops active client and server connections for this session.
    /// </summary>
    public void StopSession()
    {
        if (_networkManager.ClientManager.Started)
            _networkManager.ClientManager.StopConnection();

        if (_networkManager.ServerManager.Started)
            _networkManager.ServerManager.StopConnection(true);
    }

    private void HandleRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
            UnregisterPlayerConnection(connection);
    }

    private void HandleServerAuthenticationResult(NetworkConnection connection, bool authenticated)
    {
        if (authenticated)
            RegisterPlayerConnection(connection);
    }

    private void HandleLocalClientAuthenticated()
    {
        if (!_networkManager.ServerManager.Started)
            return;

        _localConnection = _networkManager.ClientManager.Connection;
        _hasLocalConnection = true;
        RegisterPlayerConnection(_localConnection);
    }

    private void HandleLocalClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped && _hasLocalConnection)
        {
            UnregisterPlayerConnection(_localConnection);
            _localConnection = null;
            _hasLocalConnection = false;
        }
    }

    private void RegisterPlayerConnection(NetworkConnection connection)
    {
        if (_registeredConnectionIds.Add(connection.ClientId))
            PlayerConnectionStarted?.Invoke(connection);
    }

    private void UnregisterPlayerConnection(NetworkConnection connection)
    {
        if (_registeredConnectionIds.Remove(connection.ClientId))
            PlayerConnectionStopped?.Invoke(connection);
    }
}
