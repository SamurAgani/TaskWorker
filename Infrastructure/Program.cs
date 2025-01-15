using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.ServiceHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
                  .UseWindowsService() 
                  .ConfigureServices((ctx, services) =>
                  {
                      services.AddSingleton<IWorkerRepository, InMemoryWorkerRepository>();
                      services.AddTransient<IWorkerManagementService, WorkerManagementService>();
                     
                      services.AddHostedService<WorkerManagementHostedService>();
                  })
                  .Build()
                  .Run();
    }
}