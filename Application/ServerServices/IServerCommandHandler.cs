namespace Application.ServerServices
{
    public interface IServerCommandHandler
    {
        Task HandleAsync(string request, CancellationToken token, StreamWriter writer);
    }
}
