using Application.CLIServices;
using Microsoft.Extensions.DependencyInjection;

namespace Administrative.CLI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<INamedPipeAdminClient, NamedPipeAdminClient>();
            services.AddSingleton<IAdminClient, AdminClient>();
            services.AddSingleton<IWorkerAdminService, WorkerAdminService>();
            services.AddSingleton<IServiceManager, ServiceManager>();
            services.AddSingleton<ICommandLoop, CommandLoop>();
            var serviceProvider = services.BuildServiceProvider();
            var commandLoop = serviceProvider.GetRequiredService<ICommandLoop>();
            await commandLoop.RunAsync(args);
        }
    }
}
