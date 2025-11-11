using System;

namespace LobbyService.LocalServer;

public class TcpClientMessage
{
    public Guid ClientId;
    public Message Message;
}