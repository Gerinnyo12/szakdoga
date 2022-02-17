namespace Shared
{
    public interface IWorkerTask
    {
        uint Timer { get; }
        public Task Run();
    }
}
