

namespace Application.CLIServices
{
    public interface IServiceManager
    {
        void StartService();
        Task<string> StopServiceAsync();
        Task<string> GetServiceStatusAsync();
    }
}
