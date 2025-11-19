using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyService.LocalServer;

public class Lobby
{
    public Guid Id { get; init; }
    public LocalLobbyMember Owner { get; private set; }
    public int Capacity { get; private set; }
    public LocalLobbyType LobbyType { get; private set; }
    public IReadOnlyList<LocalLobbyMember> Members => _members;
    public IReadOnlyDictionary<string, string> Metadata => _metadata;
    public IReadOnlyDictionary<Guid, Dictionary<string, string>> MemberData => _memberData;

    private readonly List<LocalLobbyMember> _members = [];
    private readonly Dictionary<string, string> _metadata = [];
    private readonly Dictionary<Guid, Dictionary<string, string>> _memberData = [];

    public Lobby(Guid lobbyId, LocalLobbyMember owner, int capacity, string name, LocalLobbyType type)
    {
        Id = lobbyId;
        Owner = owner;

        AddMember(owner);

        LobbyType = type;
        _metadata[LobbyKeys.NameKey] = name;

        if (capacity <= 0 || capacity > 100)
        {
            Console.WriteLine("Cannot set a capacity outside the bounds [1, 100]. Defaulting to 4.");
            capacity = 4;
        }

        Capacity = capacity;
    }

    private string PrintMember(LocalLobbyMember member)
    {
        var builder = new StringBuilder();

        foreach (var kvp in _memberData[member.Id])
        {
            builder.AppendLine($"{kvp.Key} = {kvp.Value}");
        }

        return $"{member}{Environment.NewLine}\t{builder}";
    }

    public override string ToString()
    {
        return
        $"Id: {Id}{Environment.NewLine}" +
        $"Owner: {Owner}" +
        $"Capacity: {Members.Count}/{Capacity}" +
        $"Type: {LobbyType}" +
        $"Memebers:" +
        $"{string.Join(Environment.NewLine + "\t", _members.Select(PrintMember))}" +
        $"Data:" +
        $"{string.Join(Environment.NewLine + "\t", _metadata.Select(kvp => $"{kvp.Key} = {kvp.Value}"))}";
    }

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
        return [.. Members.Select(m => m.Id).Where(id => !exceptions.Contains(id))];
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
        _memberData[member.Id] = [];
    }

    public void RemoveMember(LocalLobbyMember member)
    {
        _members.Remove(member);
        _memberData.Remove(member.Id);
    }

    public bool SetOwner(LocalLobbyMember newOwner)
    {
        if (newOwner == Owner) return false;
        if (!_members.Contains(newOwner)) return false;

        Owner = newOwner;
        return true;
    }

    public void SetData(string key, string value)
    {
        _metadata[key] = value;
    }

    public void SetMemberData(Guid memberId, string key, string value)
    {
        _memberData[memberId][key] = value;
    }
}