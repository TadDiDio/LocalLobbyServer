using System;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            int port = 54300;
            if (args.Length == 1)
            {
                if (int.TryParse(args[0], out var declared) && declared > 0 && declared < 65535)
                {
                    port = declared;
                }
            }
            Console.WriteLine($"Starting local lobby server on port {port}");

            MessageTypeRegistry.RegisterMessageTypes();

            var server = new Server(port);

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("Shutting down...");
                cts.Cancel();
            };

            try
            {
                await server.RunAsync(cts.Token);
            }
            catch (OperationCanceledException) { }
            finally { server.Dispose(); }
        }
    }
}