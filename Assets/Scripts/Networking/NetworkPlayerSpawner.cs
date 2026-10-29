using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

/// <summary>
/// Spawns and cleans up one registered network player for each authenticated connection.
/// </summary>
public sealed class NetworkPlayerSpawner : MonoBehaviour
{
    private const float SpawnSpacing = 2.5f;
    private const int SpawnColumns = 4;

    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private NetworkSessionCoordinator _networkSessionCoordinator;
    [SerializeField] private NetworkObject _playerPrefab;

    private readonly Dictionary<int, NetworkObject> _playersByClientId = new();
    private readonly Dictionary<int, NetworkConnection> _pendingSceneLoads = new();

    private void OnEnable()
    {
        _networkSessionCoordinator.PlayerConnectionStarted += SpawnPlayer;
        _networkSessionCoordinator.PlayerConnectionStopped += DespawnPlayer;
        _networkManager.SceneManager.OnClientLoadedStartScenes += HandleClientLoadedStartScenes;
    }

    private void OnDisable()
    {
        _networkSessionCoordinator.PlayerConnectionStarted -= SpawnPlayer;
        _networkSessionCoordinator.PlayerConnectionStopped -= DespawnPlayer;
        _networkManager.SceneManager.OnClientLoadedStartScenes -= HandleClientLoadedStartScenes;
        foreach (NetworkConnection connection in _pendingSceneLoads.Values)
            connection.OnLoadedStartScenes -= HandleConnectionLoadedStartScenes;
        _pendingSceneLoads.Clear();
    }

    private void SpawnPlayer(NetworkConnection connection)
    {
        if (!_networkManager.ServerManager.Started || _playersByClientId.ContainsKey(connection.ClientId))
            return;

        if (!connection.LoadedStartScenes(true))
        {
            _pendingSceneLoads[connection.ClientId] = connection;
            connection.OnLoadedStartScenes += HandleConnectionLoadedStartScenes;
            return;
        }

        SpawnAfterSceneLoad(connection);
    }

    private void HandleConnectionLoadedStartScenes(NetworkConnection connection, bool asServer)
    {
        if (!asServer)
            return;

        connection.OnLoadedStartScenes -= HandleConnectionLoadedStartScenes;
        if (_pendingSceneLoads.Remove(connection.ClientId) && connection.IsActive)
            SpawnAfterSceneLoad(connection);
    }

    private void HandleClientLoadedStartScenes(NetworkConnection connection, bool asServer)
    {
        if (!asServer || !_pendingSceneLoads.Remove(connection.ClientId))
            return;

        connection.OnLoadedStartScenes -= HandleConnectionLoadedStartScenes;
        if (connection.IsActive)
            SpawnAfterSceneLoad(connection);
    }

    private void SpawnAfterSceneLoad(NetworkConnection connection)
    {
        if (_playersByClientId.ContainsKey(connection.ClientId))
            return;

        Vector3 position = new Vector3(connection.ClientId % SpawnColumns * SpawnSpacing, 0f, connection.ClientId / SpawnColumns * SpawnSpacing);
        NetworkObject player = _networkManager.GetPooledInstantiated(_playerPrefab, position, Quaternion.identity, true);
        _networkManager.ServerManager.Spawn(player, connection);
        _networkManager.SceneManager.AddOwnerToDefaultScene(player);
        _playersByClientId.Add(connection.ClientId, player);
    }

    private void DespawnPlayer(NetworkConnection connection)
    {
        _pendingSceneLoads.Remove(connection.ClientId);
        connection.OnLoadedStartScenes -= HandleConnectionLoadedStartScenes;
        if (!_playersByClientId.TryGetValue(connection.ClientId, out NetworkObject player))
            return;

        _playersByClientId.Remove(connection.ClientId);
        if (player.IsSpawned)
            _networkManager.ServerManager.Despawn(player);
    }
}
