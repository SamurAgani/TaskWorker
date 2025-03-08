
using System.Diagnostics;

namespace Application.CLIServices
{
    public class ServiceManager : IServiceManager
    {
        private readonly string _servicePath;
        public IAdminClient namedPipeAdminClient { get; set; }
        public ServiceManager(IAdminClient NamedPipeAdminClient)
        {
            namedPipeAdminClient = NamedPipeAdminClient;
            _servicePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\..\Server\bin\Debug\net8.0\Server.exe"));
        }
        public void StartService()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _servicePath,
                WorkingDirectory = Path.GetDirectoryName(_servicePath) ?? "",
                UseShellExecute = true
            });
        }
        public async Task<string> StopServiceAsync() => await namedPipeAdminClient.SendCommandAsync("STOP_SERVER");
        public async Task<string> GetServiceStatusAsync() => await namedPipeAdminClient.SendCommandAsync("STATUS");
    }
}
