using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyService.LocalServer;

public class LobbyManager
{
    public readonly LobbyBrowser Browser;

    public readonly List<LocalLobbyMember> ConnectedMembers = [];
    private readonly Dictionary<LocalLobbyMember, Guid> _memberToLobby = [];
    private readonly Dictionary<Guid, Lobby> _lobbies = [];

    private LocalLobbyServer _messager;

    public LobbyManager(LocalLobbyServer server)
    {
        Browser = new LobbyBrowser(this);
        _messager = server;
    }
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

        if (_memberToLobby.TryGetValue(member, out var lobbyId))
        {
            InternalRemove(lobbyId, member, 0);
        }
    }

    public EnterResponse CreateLobby(CreateLobbyRequest request, LocalLobbyMember sender)
    {
        if (_memberToLobby.TryGetValue(sender, out var oldLobbyId))
        {
            InternalRemove(oldLobbyId, sender, 0);
        }

        var lobbyId = Guid.NewGuid();
        var lobby = new Lobby(lobbyId, sender, request.Capacity, request.Name, request.LobbyType);

        _lobbies[lobbyId] = lobby;
        _memberToLobby[sender] = lobbyId;

        Console.WriteLine($"{sender} created lobby {lobbyId}");

        return new EnterResponse
        {
            Snapshot = lobby.GetSnapshot()
        };
    }

    public Lobby JoinLobby(JoinLobbyRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var id)) return null;
        if (!_lobbies.TryGetValue(id, out var lobby)) return null;
        if (lobby.Members.Count >= lobby.Capacity) return null;

        if (_memberToLobby.TryGetValue(sender, out var oldLobbyId))
        {
            InternalRemove(oldLobbyId, sender, 0);
        }


        InternalAdd(id, sender);

        return _lobbies[id];
    }

    public void LeaveLobby(LeaveLobbyRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        InternalRemove(lobbyId, sender, 0);
    }

    public void CloseLobby(CloseLobbyRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return;

        for (int i = lobby.Members.Count - 1; i >= 0; i--)
        {
            InternalKick(lobbyId, lobby.Members[i].Id, 1);
        }
    }

    public void Invite(InviteMemberRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!ValidId(request.InviteeId, out var inviteeId)) return;

        var invitee = ConnectedMembers.FirstOrDefault(m => m.Id == inviteeId);
        if (invitee == null) return;

        if (_memberToLobby.TryGetValue(invitee, out var memberLobbyId) && _lobbies.TryGetValue(memberLobbyId, out var receiverLobby))
        {
            if (receiverLobby.Id == lobbyId)
            {
                Console.WriteLine($"Ignoring invite because {invitee} is already in {sender}'s lobby");
                return;
            } 
        }

        _messager.SendMessage(inviteeId, Message.CreateEvent(new ReceivedInviteEvent
        {
            Sender = sender,
            LobbyId = lobbyId
        }));

        Console.WriteLine($"{sender} invited {invitee} to lobby {lobbyId}");
    }

    public void Kick(KickMemberRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!ValidId(request.KickeeId, out var kickeeId)) return;
        if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return;

        if (sender != lobby.Owner) return;
        if (kickeeId == lobby.Owner.Id) return; // Disallow owner kicking self

        InternalKick(lobbyId, kickeeId, 0);
    }

    public void SetLobbyData(LobbyDataRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return;
        if (!lobby.Owner.Equals(sender)) return;

        lobby.SetData(request.Key, request.Value);
        _messager.Broadcast(lobby.GetReceiversExcept(), Message.CreateEvent(new LobbyDataUpdateEvent
        {
            Metadata = lobby.Metadata.ToDictionary()
        }));
    }

    public void SetMemberData(MemberDataRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return;
        if (!lobby.Members.Contains(sender)) return;

        lobby.SetMemberData(sender.Id, request.Key, request.Value);
        
        _messager.Broadcast(lobby.GetReceiversExcept(), Message.CreateEvent(new MemberDataUpdateEvent
        {
            Member = sender,
            Metadata = lobby.MemberData[sender.Id].ToDictionary()
        }));
    }

    public void SendChat(ChatMessageRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return;
        if (!lobby.Members.Contains(sender)) return;

        _messager.Broadcast(lobby.GetReceiversExcept(), Message.CreateEvent(new ChatEvent
        {
            Content = request.Message,
            Sender = sender,
            Direct = false
        }));
    }

    public void SendDirectChat(DirectMessageRequest request, LocalLobbyMember sender)
    {
        if (!ValidId(request.LobbyId, out var lobbyId)) return;
        if (!ValidId(request.TargetId, out var targetId)) return;
        if (!_lobbies.TryGetValue(lobbyId, out var lobby)) return;

        if (!lobby.Members.Contains(sender)) return;
        if (!lobby.Members.Any(m => m.Id == targetId)) return;

        _messager.Broadcast([sender.Id, targetId], Message.CreateEvent(new ChatEvent
        {
            Content = request.Message,
            Sender = sender,
            Direct = true
        }));
    }

    private void InternalAdd(Guid lobbyId, LocalLobbyMember member)
    {
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
        {
            Console.WriteLine($"Could not find lobby {lobbyId}");
            return;
        }

        lobby.AddMember(member);
        _memberToLobby[member] = lobbyId;

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
    private void InternalRemove(Guid lobbyId, LocalLobbyMember member, int reason, int kickReason = -1)
    {
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
        {
            Console.WriteLine($"Could not find lobby {lobbyId}");
            return;
        }

        lobby.RemoveMember(member);
        _memberToLobby.Remove(member);

        Console.WriteLine($"{member} left lobby {lobbyId}");

        if (lobby.Members.Count == 0)
        {
            _lobbies.Remove(lobbyId);
            return;
        }
        
        _messager.Broadcast(lobby.GetReceiversExcept(), Message.CreateEvent(new OtherMemberLeftEvent
        {
            Member = member,
            LeaveReason = reason,
            KickReason = kickReason
        }));

        if (lobby.Owner == member)
        {
            SetOwner(lobbyId.ToString(), lobby.Members[0].Id.ToString());
        }
    }

    /// <summary>
    /// Kicks a member form a lobby and notifies relevant parties.
    /// </summary>
    /// <param name="lobbyId">The lobby to kick from.</param>
    /// <param name="kickeeId">The member to kick.</param>
    /// <param name="kickReason">0 = general, 1 = lobby closed, 2 = owner stopped responding</param>
    /// <remarks>Broadcast message depends on kick reason.</remarks>
    private void InternalKick(Guid lobbyId, Guid kickeeId, int kickReason)
    {
        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
        {
            Console.WriteLine($"Could not find lobby {lobbyId}");
            return;
        }

        var kickee = lobby.Members.FirstOrDefault(m => m.Id == kickeeId);

        if (kickee == null)
        {
            Console.WriteLine($"Cannot kick {ConnectedMembers.FirstOrDefault(m => m.Id == kickeeId)} because they are not a member of {lobbyId}");
            return;
        }

        lobby.RemoveMember(kickee);
        _memberToLobby.Remove(kickee);

        _messager.SendMessage(kickeeId, Message.CreateEvent(new LocalMemberKickedEvent
        {
            KickReason = kickReason
        }));

        Console.WriteLine($"{kickee} was kicked from {lobbyId}");

        if (lobby.Members.Count == 0)
        {
            _lobbies.Remove(lobbyId);
            return;
        }
        
        // Only notify others when a single user is being kicked
        if (kickReason == 0)
        {
            _messager.Broadcast(lobby.GetReceiversExcept(), Message.CreateEvent(new OtherMemberLeftEvent
            {
                Member = kickee,
                LeaveReason = 1,
                KickReason = kickReason
            }));   
        }
    }

    public void SetOwner(string lobbyIdStr, string newOwnerIdStr)
    {
        if (!ValidId(lobbyIdStr, out var lobbyId)) return;
        if (!ValidId(newOwnerIdStr, out var newOwnerId)) return;

        if (!_lobbies.TryGetValue(lobbyId, out var lobby))
        {
            Console.WriteLine($"Could not find lobby {lobbyId}");
            return;
        }

        var newOwner = lobby.Members.FirstOrDefault(m => m.Id == newOwnerId);

        if (newOwner == null)
        {
            Console.WriteLine($"Could not make {newOwnerId} the owner because they are not in the lobby.");
        }

        if (!lobby.SetOwner(newOwner))
        {
            Console.WriteLine($"Failed to make {newOwner} the owner of {lobbyId}");
            return;
        }

        _messager.Broadcast(lobby.GetReceiversExcept(), Message.CreateEvent(new OwnerUpdateEvent
        {
            NewOwner = lobby.Owner
        }));

        Console.WriteLine($"{lobby.Owner} was promoted to owner of {lobbyId}");
    }

    public List<LobbySnapshot> GetAllLobbies()
    {
        var result = new List<LobbySnapshot>();

        foreach (var lobby in _lobbies.Values)
        {
            result.Add(lobby.GetSnapshot());    
        }

        return result;
    }
}
