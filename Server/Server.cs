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
        _clients[clientId] = new LocalLobbyMember(clientId, NameGenerator.New);
        Console.WriteLine($"{_clients[clientId]} connected");
    }

    private void HandleTcpClientDisconnected(Guid clientId)
    {
        Console.WriteLine($"{_clients[clientId]} disconnected");
        _clients.Remove(clientId);
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