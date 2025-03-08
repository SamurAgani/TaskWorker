using Application.WorkerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Pipes;

namespace Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Guid workerId = (args.Length > 0 && Guid.TryParse(args[0], out var parsed))
                ? parsed
                : Guid.NewGuid();

            string pipeName = $"WorkerPipe-{workerId}";
            Console.WriteLine($"Worker started. ID = {workerId}");
            var host = CreateHostBuilder(args).Build();

            var pipeHandler = host.Services.GetRequiredService<IWorkerPipeHandler>();

            while (true)
            {
                var pipeServer = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous,
                    0,
                    0);

                await pipeServer.WaitForConnectionAsync();
                _ = Task.Run(() => pipeHandler.HandleConnectionAsync(pipeServer));
            }
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureServices((context, services) =>
               {
                   services.AddSingleton<IWorkerCommandExecutor, WorkerCommandExecutor>();
                   services.AddSingleton<IWorkerPipeHandler, WorkerPipeHandler>();
               });
      
    }
}
