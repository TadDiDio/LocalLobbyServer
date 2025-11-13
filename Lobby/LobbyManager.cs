using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyService.LocalServer;

public class LobbyManager
{
    public readonly List<LocalLobbyMember> ConnectedMembers = [];
    public readonly Dictionary<LocalLobbyMember, Guid> MemberToLobby = [];
    public readonly Dictionary<Guid, Lobby> Lobbies = [];

    private LocalLobbyServer _messager;

    public LobbyManager(LocalLobbyServer server) => _messager = server;

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
            Remove(lobbyId, member, 0);
        }
    }

    public EnterResponse CreateLobby(CreateLobbyRequest request, LocalLobbyMember sender)
    {
        if (MemberToLobby.TryGetValue(sender, out var oldLobbyId))
        {
            Remove(oldLobbyId, sender, 0);
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

    public Lobby JoinLobby(JoinLobbyRequest request, LocalLobbyMember sender)
    {
        if (MemberToLobby.TryGetValue(sender, out var oldLobbyId))
        {
            Remove(oldLobbyId, sender, 0);
        }

        if (!ValidId(request.LobbyId, out var id)) return null;

        Add(id, sender);

        return Lobbies[id];
    }

    public void LeaveLobby(LeaveLobbyRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        Remove(lobbyId, sender, 0);
    }

    public void Invite(InviteMemberRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!ValidId(request.InviteeId, out var inviteeId)) return;

        var invitee = ConnectedMembers.FirstOrDefault(m => m.Id == inviteeId);
        if (invitee == null) return;

        _messager.SendMessage(inviteeId, Message.CreateEvent(new ReceivedInviteEvent
        {
            Sender = sender,
            LobbyId = lobbyId
        }));

        Console.WriteLine($"{sender} invited {invitee} to lobby {lobbyId}");
    }

    private void Add(Guid lobbyId, LocalLobbyMember member)
    {
        if (!Lobbies.TryGetValue(lobbyId, out var lobby))
        {
            Console.WriteLine($"Could not find lobby {lobbyId}");
            return;
        }

        lobby.AddMember(member);
        MemberToLobby[member] = lobbyId;

        _messager.Broadcast(lobby.GetReceiversExcept(member.Id), Message.CreateEvent(new OtherMemberJoinedEvent
        {
            Member = member,
            Metadata = []
        }));

        Console.WriteLine($"{member} joined lobby {lobbyId}");
    }

    /// <summary>
    /// Removes a member and notifies other members.
    /// </summary>
    /// <param name="reason">0 = user requested, 1 = kicked</param>
    /// <param name="kickReason">0 = general, 1 = lobby closed, 2 = owner stopped responding</param>
    private void Remove(Guid lobbyId, LocalLobbyMember member, int reason, int kickReason = -1)
    {
        if (!Lobbies.TryGetValue(lobbyId, out var lobby))
        {
            Console.WriteLine($"Could not find lobby {lobbyId}");
            return;
        }

        lobby.RemoveMember(member);
        MemberToLobby.Remove(member);

        if (lobby.Members.Count == 0)
        {
            Lobbies.Remove(lobbyId);
        }

        _messager.Broadcast(lobby.GetReceiversExcept(), Message.CreateEvent(new OtherMemberLeftEvent
        {
            Member = member,
            LeaveReason = reason,
            KickReason = kickReason
        }));

        Console.WriteLine($"{member} left lobby {lobbyId}");
    }
}
