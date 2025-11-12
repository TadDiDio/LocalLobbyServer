using System;
using System.Collections.Generic;
using System.Linq;

namespace LobbyService.LocalServer;

public class Lobby
{
    public Guid Id { get; init; }
    public LocalLobbyMember Owner { get; private set; }
    public int Capacity { get; private set; }
    public LocalLobbyType LobbyType { get; private set; }
    public IReadOnlyList<LocalLobbyMember> Members => _members;
    public IReadOnlyDictionary<string, string> Metadata => _metadata;
    public IReadOnlyDictionary<LocalLobbyMember, Dictionary<string, string>> MemberData => _memberData;

    public event Action<LocalLobbyMember> OnNewOwner;
    public event Action OnClosed;
    public event Action<LocalLobbyMember> OnMemberLeft;

    private readonly List<LocalLobbyMember> _members = [];
    private readonly Dictionary<string, string> _metadata = [];
    private readonly Dictionary<LocalLobbyMember, Dictionary<string, string>> _memberData = [];

    public LobbySnapshot GetSnapshot()
    {
        return new LobbySnapshot
        {
            LobbyId = Id,
            Owner = Owner.Copy(),
            Capacity = Capacity,
            LobbyType = LobbyType,
            Members = Members,
            LobbyData = Metadata,
            MemberData = MemberData,
        };
    }

    public List<Guid> GetReceiversExcept(params Guid[] exceptions)
    {
        return [.. Members.Select(m => m.Id).Where(id => !exceptions.Contains(Id))];
    }

    public Lobby(Guid lobbyId, LocalLobbyMember ownerId, int capacity)
    {
        Id = lobbyId;
        Owner = ownerId;

        if (capacity <= 0 || capacity > 100)
        {
            Console.WriteLine("Cannot set a capacity outside the bounds [1, 100]. Defaulting to 4.");
            capacity = 4;
        }

        Capacity = capacity;
    }

    public void SetCapacity(int capacity)
    {
        if (capacity <= 0 || capacity > 100)
        {
            Console.WriteLine("Cannot set a capacity outside the bounds [1, 100]");
            return;
        }

        Capacity = capacity;
    }

    public void AddMember(LocalLobbyMember member)
    {
        if (_members.Contains(member))
        {
            Console.WriteLine("Attempted to add a member that was already in this lobby!");
            return;
        }

        _members.Add(member);
    }

    public void RemoveMember(LocalLobbyMember member)
    {
        _members.Remove(member);
        OnMemberLeft?.Invoke(member);

        if (_members.Count == 0)
        {
            Close();
        }
        else if (member == Owner)
        {
            SetOwner(_members[0]);
        }
    }

    public void Close()
    {
        _members.Clear();
        Owner = null;
        OnClosed?.Invoke();
    }

    public void SetOwner(LocalLobbyMember newOwner)
    {
        if (newOwner == Owner) return;
        if (!_members.Contains(newOwner)) return;

        Owner = newOwner;
        OnNewOwner?.Invoke(newOwner);
    }
}