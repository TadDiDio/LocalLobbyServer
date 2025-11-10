using System;
using System.Threading;

namespace LobbyService.LocalServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Starting local lobby server on port {ServerDetails.Port}");

            CommandTypeRegistry.RegisterCommandTypes();

            var server = new Server(ServerDetails.Port);
            server.RunAsync(CancellationToken.None).Wait();
        }
    }
}