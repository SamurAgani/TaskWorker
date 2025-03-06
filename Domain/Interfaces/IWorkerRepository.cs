namespace Domain.Interfaces
{
    public interface IWorkerRepository
    {
        IEnumerable<Entities.Worker> GetWorkers();
        Entities.Worker? GetWorkerById(Guid id);
        Entities.Worker AddWorker(Entities.Worker worker);
        void RemoveWorker(Guid id);
        void UpdateWorker(Entities.Worker worker);
        void UpdateTaskResult(Guid workerId, Guid id, string result);

    }
}
