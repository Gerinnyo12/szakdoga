namespace Shared.Interfaces
{
    public interface IWorkerTask
    {
        uint Timer { get; }
        public Task Run();
    }
}
