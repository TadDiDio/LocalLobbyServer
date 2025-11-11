using System;

namespace LobbyService.LocalServer;

public class CallbackHandle
{
    public Action<Message> OnMessage;
    public Action OnDisconnect;
}