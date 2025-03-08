using Server.ServiceHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Application.ServerServices;

internal class Program
{
    public static void Main(string[] args)
    {
        bool createdNew;
        using (var mutex = new Mutex(true, "MyServiceMutex", out createdNew))
        {
            if (!createdNew)
            {
                return;
            }
            Host.CreateDefaultBuilder(args)
                  .UseWindowsService()
                  .ConfigureServices((ctx, services) =>
                  {
                      services.AddSingleton<IWorkerRepository, InMemoryWorkerRepository>();
                      services.AddTransient<IWorkerManagementService, WorkerManagementService>();
                      services.AddTransient<IServerCommandHandler, ServerCommandHandler>();

                      services.AddHostedService<WorkerManagementHostedService>();
                  })
                  .Build()
                  .Run();
        }
    }
}