using System;
using Steamworks;
using UnityEngine;

/// <summary>
/// Owns Steam lobby creation, joining, leaving, and the handoff to FishNet transport sessions.
/// </summary>
public sealed class SteamLobbySession : MonoBehaviour
{
    private const ELobbyType LobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
    private const string HostSteamIdKey = "hostSteamId";

    [SerializeField] private NetworkSessionCoordinator _networkSessionCoordinator;

    private ulong _currentLobbySteamId;
    private CallResult<LobbyCreated_t> _createLobbyResult;
    private CallResult<LobbyEnter_t> _joinLobbyResult;

    /// <summary>
    /// Creates a friends-only Steam lobby and starts FishNet as host after Steam confirms creation.
    /// </summary>
    public void CreateLobby(int maxMembers)
    {
        RequireInitializedSteam();
        _createLobbyResult = CallResult<LobbyCreated_t>.Create(HandleLobbyCreated);
        _createLobbyResult.Set(SteamMatchmaking.CreateLobby(LobbyType, maxMembers));
    }

    /// <summary>
    /// Joins the requested Steam lobby and starts FishNet as a client after Steam confirms entry.
    /// </summary>
    public void JoinLobby(ulong lobbySteamId)
    {
        RequireInitializedSteam();
        _joinLobbyResult = CallResult<LobbyEnter_t>.Create(HandleLobbyEntered);
        _joinLobbyResult.Set(SteamMatchmaking.JoinLobby(new CSteamID(lobbySteamId)));
    }

    /// <summary>
    /// Stops FishNet and leaves the current Steam lobby, if one is active.
    /// </summary>
    public void LeaveLobby()
    {
        _networkSessionCoordinator.StopSession();

        if (_currentLobbySteamId != 0)
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(_currentLobbySteamId));
            _currentLobbySteamId = 0;
        }
    }

    private void HandleLobbyCreated(LobbyCreated_t result, bool ioFailure)
    {
        if (ioFailure || result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Steam lobby creation failed.", this);
            return;
        }

        _currentLobbySteamId = result.m_ulSteamIDLobby;
        var lobbyId = new CSteamID(_currentLobbySteamId);
        SteamMatchmaking.SetLobbyData(lobbyId, HostSteamIdKey, SteamUser.GetSteamID().m_SteamID.ToString());
        _networkSessionCoordinator.StartHost();
    }

    private void HandleLobbyEntered(LobbyEnter_t result, bool ioFailure)
    {
        if (ioFailure || result.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Debug.LogError("Joining the Steam lobby failed.", this);
            return;
        }

        _currentLobbySteamId = result.m_ulSteamIDLobby;
        var lobbyId = new CSteamID(_currentLobbySteamId);
        var hostSteamId = SteamMatchmaking.GetLobbyData(lobbyId, HostSteamIdKey);
        _networkSessionCoordinator.StartClient(hostSteamId);
    }

    private static void RequireInitializedSteam()
    {
        if (!SteamManager.Initialized)
            throw new InvalidOperationException("Steamworks must be initialized before using Steam lobbies.");
    }
}
