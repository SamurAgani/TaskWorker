
namespace Application.CLIServices
{
    public interface IAdminClient
    {
        Task<string> SendCommandAsync(string command);

    }
}
