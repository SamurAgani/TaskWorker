namespace Application.CLIServices
{
    public interface ICommandLoop
    {
        public Task RunAsync(string[] args);
    }
}
