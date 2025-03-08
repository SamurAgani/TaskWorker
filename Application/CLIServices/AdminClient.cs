namespace Application.CLIServices
{
    public class AdminClient : IAdminClient
    {
        public INamedPipeAdminClient namedPipeAdminClient;
        public AdminClient(INamedPipeAdminClient NamedPipeAdminClient)
        {
            namedPipeAdminClient = NamedPipeAdminClient;
        }
        public Task<string> SendCommandAsync(string command) => namedPipeAdminClient.SendCommandAsync(command);
    }

}
