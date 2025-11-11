using System;
using System.Collections.Generic;

public class Lobby
{
    public Guid Id { get; init; }

    public Guid OwnerId;

    public readonly List<Guid> Members = [];

    public readonly Dictionary<string, string> Metadata = [];
}