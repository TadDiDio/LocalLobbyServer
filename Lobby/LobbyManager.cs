using System;
using System.Collections.Generic;

namespace LobbyService.LocalServer;

public class LobbyManager
{
    public readonly List<LocalLobbyMember> Members = [];
    public readonly Dictionary<LocalLobbyMember, Guid> MemberToLobby = [];
    public readonly Dictionary<Guid, Lobby> Lobbies = [];

    private bool ValidId(string id, out Guid guid)
    {
        return Guid.TryParse(id, out guid);
    }

    public void RegisterMember(LocalLobbyMember member)
    {
        if (Members.Contains(member))
        {
            Console.WriteLine($"Failed to register client {member} because they already existed");
            return;
        }

        Members.Add(member);
        MemberToLobby[member] = Guid.Empty;
    }

    public void UnregisterMember(LocalLobbyMember member)
    {
        Members.Remove(member);

        if (MemberToLobby.TryGetValue(member, out var lobbyId))
        {
            Lobbies[lobbyId].RemoveMember(member);
        }
        MemberToLobby.Remove(member);
    }

    public EnterResponse CreateLobby(CreateLobbyRequest request, LocalLobbyMember sender)
    {
        if (MemberToLobby.TryGetValue(sender, out var oldLobbyId))
        {
            Lobbies[oldLobbyId].RemoveMember(sender);
        }

        var lobbyId = Guid.NewGuid();
        var lobby = new Lobby(lobbyId, sender, request.Capacity);

        Lobbies[lobbyId] = lobby;
        MemberToLobby[sender] = lobbyId;

        return new EnterResponse
        {
            Snapshot = lobby.GetSnapshot()
        };
    }

    public void LeaveLobby(LeaveLobbyRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;

        Lobbies[lobbyId].RemoveMember(sender);
        MemberToLobby[sender] = Guid.Empty;
    }
}