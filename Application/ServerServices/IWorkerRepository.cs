using Domain.Entities;

namespace Application.ServerServices
{
    public interface IWorkerRepository
    {
        IEnumerable<Worker> GetWorkers();
        Worker? GetWorkerById(Guid id);
        Worker AddWorker(Worker worker);
        void RemoveWorker(Guid id);
        void UpdateWorker(Worker worker);
        void UpdateTaskResult(Guid workerId, Guid id, string result);

    }
}
