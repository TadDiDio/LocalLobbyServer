using System;

namespace LobbyService.LocalServer;

public struct CommandContext
{
    public Guid RequestId;
    public LocalLobbyMember Sender;
    public LobbyManager LobbyManager;
}