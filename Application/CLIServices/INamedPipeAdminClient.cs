
namespace Application.CLIServices
{
    public interface INamedPipeAdminClient
    {
        public Task<string> SendCommandAsync(string command);

    }
}
