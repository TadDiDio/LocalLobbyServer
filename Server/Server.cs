using System;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class Server : IDisposable
    {
        private int _port;
        private LobbyManager _lobbyManager;
        private LocalLobbyServer _lobbyServer;

        public Server(int port)
        {
            _port = port;
            _lobbyServer = new();
            _lobbyManager = new();
        }

        public async Task RunAsync(CancellationToken token)
        {
            _lobbyServer.OnClientConnect += HandleClientConnected;
            _lobbyServer.OnClientDisconnect += HandleClientDisconnected;
            _lobbyServer.OnClientMessage += HandleClientMessage;

            await _lobbyServer.RunAsync(_port, token);
        }

        private void HandleClientConnected(Guid clientId)
        {
            Console.WriteLine($"Client {clientId} connected");
        }

        private void HandleClientDisconnected(Guid clientId)
        {
            Console.WriteLine($"Client {clientId} disconnected");
        }

        private void HandleClientMessage(ClientMessage clientMessage)
        {
            var message = clientMessage.Message;

            Console.WriteLine($"Received message: {message.Type}");

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

            var response = Execute(command, message.RequestId);

            Console.WriteLine("Sending response");
            _lobbyServer.SendMessage(clientMessage.ClientId, response);
        }

        private Message Execute(ICommand command, string requestId)
        {
            var ctx = new CommandContext
            {
                RequestId = requestId,
                LobbyManager = _lobbyManager
            };

            return command.Execute(ctx);
        }

        public void Dispose()
        {
            _lobbyServer?.Dispose();
        }
    }
}