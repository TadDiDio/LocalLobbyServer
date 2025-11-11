using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer;


public class LocalLobbyServer : IDisposable
{
    public event Action<TcpClientMessage> OnClientMessage;
    public event Action<Guid> OnTcpClientConnect;
    public event Action<Guid> OnClientDisconnect;

    private TcpListener _listener;
    private Dictionary<Guid, TcpClient> _clients;
    private Dictionary<Guid, MessageReader> _readers;
    private Dictionary<Guid, MessageWriter> _writers;
    private Dictionary<Guid, CallbackHandle> _handles;

    public async Task RunAsync(int port, CancellationToken token)
    {
        _clients = [];
        _readers = [];
        _writers = [];
        _handles = [];

        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();

        Console.WriteLine($"Server listening on port {port}");

        while (!token.IsCancellationRequested)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync().AsCancellable(token);
                HandleTcpConnect(client);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex) { Console.WriteLine($"Accept failed: {ex.Message}"); }
        }

        _listener.Stop();

        Console.WriteLine("Server stopped.");
    }

    public void SendMessage(Guid targetId, Message message)
    {
        if (!_writers.ContainsKey(targetId))
        {
            Console.WriteLine($"Could not find a writer for client {targetId}!");
            return;
        }

        _writers[targetId].Send(message);
    }

    private void HandleTcpConnect(TcpClient client)
    {
        if (client == null) return;

        Guid clientId = Guid.NewGuid();

        if (_clients.ContainsKey(clientId))
        {
            Console.WriteLine("Attempted to add a client that was already recorded");
            return;
        }

        _clients[clientId] = client;

        var stream = client.GetStream();

        _readers[clientId] = new MessageReader(new StreamReader(stream, Encoding.UTF8));
        _writers[clientId] = new MessageWriter(new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true });

        var handle = new CallbackHandle
        {
            OnMessage = msg => HandleMessage(clientId, msg),
            OnDisconnect = () => HandleTcpDisconnect(clientId)
        };

        _handles[clientId] = handle;
        _readers[clientId].OnMessage += handle.OnMessage;
        _readers[clientId].OnDisconnected += handle.OnDisconnect;

        OnTcpClientConnect?.Invoke(clientId);
    }

    private void HandleMessage(Guid clientId, Message message)
    {
        OnClientMessage?.Invoke(new TcpClientMessage { ClientId = clientId, Message = message });
    }

    private void HandleTcpDisconnect(Guid clientId)
    {
        RemoveTcpClient(clientId);
        OnClientDisconnect?.Invoke(clientId);
    }

    public void Dispose()
    {
        foreach (var clientId in _clients.Keys) RemoveTcpClient(clientId);
    }

    private void RemoveTcpClient(Guid id)
    {
        if (_writers.ContainsKey(id))
        {
            _writers[id]?.Dispose();
            _writers.Remove(id);
        }

        if (_handles.ContainsKey(id))
        {
            if (_readers.ContainsKey(id))
            {
                _readers[id].OnMessage -= _handles[id].OnMessage;
                _readers[id].OnDisconnected -= _handles[id].OnDisconnect;
            }

            _handles.Remove(id);
        }

        if (_readers.ContainsKey(id))
        {
            _readers[id]?.Dispose();
            _readers.Remove(id);
        }

        var client = _clients[id];

        _clients.Remove(id);
        client?.Close();
        client?.Dispose();
    }
}