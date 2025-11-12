using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer;

public class Server : IDisposable
{
    private int _port;
    private LobbyManager _lobbyManager;
    private LocalLobbyServer _lobbyServer;
    private Dictionary<Guid, LocalLobbyMember> _clients;

    public Server(int port)
    {
        _port = port;
        _lobbyServer = new();
        _lobbyManager = new(_lobbyServer);
        _clients = [];
    }

    public async Task RunAsync(CancellationToken token)
    {
        _lobbyServer.OnTcpClientConnect += HandleTcpClientConnected;
        _lobbyServer.OnClientDisconnect += HandleTcpClientDisconnected;
        _lobbyServer.OnClientMessage += HandleTcpClientMessage;

        await _lobbyServer.RunAsync(_port, token);
    }

    private void HandleTcpClientConnected(Guid clientId)
    {
        var member = new LocalLobbyMember(clientId, NameGenerator.New);

        _clients[clientId] = member;
        _lobbyManager.RegisterMember(member);
        
        Console.WriteLine($"{member} connected");
    }

    private void HandleTcpClientDisconnected(Guid clientId)
    {
        var member = _clients[clientId];

        _lobbyManager.UnregisterMember(member);
        _clients.Remove(clientId);

        Console.WriteLine($"{member} disconnected");
    }

    private void HandleTcpClientMessage(TcpClientMessage clientMessage)
    {
        var message = clientMessage.Message;

        if (!MessageTypeRegistry.TryGetType(message.Type, out var type))
        {
            _lobbyServer.SendMessage(clientMessage.ClientId, Message.CreateFailure(Error.Serialization, message.RequestId));
            return;
        }

        if (message.Payload.ToObject(type) is not IRequest request)
        {
            _lobbyServer.SendMessage(clientMessage.ClientId, Message.CreateFailure(Error.Serialization, message.RequestId));
            return;
        }

        if (!CommandRegistry.TryGet(request, out var command))
        {
            Console.WriteLine($"No command can handle request with type '{message.Type}'");
            return;
        }

        var context = new CommandContext
        {
            LobbyManager = _lobbyManager,
            Sender = _clients[clientMessage.ClientId],
            RequestId = message.RequestId
        };

        var response = command.Execute(context);

        _lobbyServer.SendMessage(clientMessage.ClientId, response);
    }


    public void Dispose()
    {
        _lobbyServer?.Dispose();
    }
}