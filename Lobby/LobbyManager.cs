using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyService.LocalServer;

public class LobbyManager
{
    public readonly List<LocalLobbyMember> ConnectedMembers = [];
    public readonly Dictionary<LocalLobbyMember, Guid> MemberToLobby = [];
    public readonly Dictionary<Guid, Lobby> Lobbies = [];
    
    private LocalLobbyServer _server;

    public LobbyManager(LocalLobbyServer server) => _server = server;

    private bool ValidId(string id, out Guid guid)
    {
        return Guid.TryParse(id, out guid);
    }

    public void RegisterMember(LocalLobbyMember member)
    {
        if (ConnectedMembers.Contains(member))
        {
            Console.WriteLine($"Failed to register client {member} because they already existed");
            return;
        }

        ConnectedMembers.Add(member);
    }

    public void UnregisterMember(LocalLobbyMember member)
    {
        ConnectedMembers.Remove(member);

        if (MemberToLobby.TryGetValue(member, out var lobbyId))
        {
            Lobbies[lobbyId].RemoveMember(member);
            MemberToLobby.Remove(member);
        }
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

        Console.WriteLine($"{sender} created lobby {lobbyId}");

        return new EnterResponse
        {
            Snapshot = lobby.GetSnapshot()
        };
    }

    public void LeaveLobby(LeaveLobbyRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!Lobbies.TryGetValue(lobbyId, out var lobby)) return;

        lobby.RemoveMember(sender);
        MemberToLobby.Remove(sender);

        _server.Broadcast(lobby.GetReceiversExcept(sender.Id), Message.CreateEvent(new OtherMemberLeftEvent
        {
            Member = sender,
            LeaveReason = 0,
            KickReason = -1
        }));

        Console.WriteLine($"{sender} left lobby {request.LobbyId}");
    }

    public void Invite(InviteMemberRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!ValidId(request.InviteeId, out var inviteeId)) return;

        var invitee = ConnectedMembers.FirstOrDefault(m => m.Id == inviteeId);
        if (invitee == null) return;

        _server.SendMessage(inviteeId, Message.CreateEvent(new ReceivedInviteEvent
        {
            Sender = sender,
            LobbyId = lobbyId
        }));

        Console.WriteLine($"{sender} invited {invitee} to lobby {lobbyId}");
    }
}