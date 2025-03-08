using System.IO.Pipes;

namespace Application.WorkerServices
{
    public interface IWorkerPipeHandler
    {
        Task HandleConnectionAsync(NamedPipeServerStream pipeServer);
    }
}
