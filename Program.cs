using System;
using System.Threading;
using System.Threading.Tasks;

namespace LobbyService.LocalServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Starting local lobby server on port {ServerDetails.Port}");

            MessageTypeRegistry.RegisterMessageTypes();

            var server = new Server(ServerDetails.Port);

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
            catch (OperationCanceledException)
            {
                /* Expected on Ctrl+C */
            }
            finally
            {
                server.Dispose();
            }
        }
    }
}