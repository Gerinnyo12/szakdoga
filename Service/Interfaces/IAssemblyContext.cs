namespace Service.Interfaces
{
    public interface IAssemblyContext
    {
        IRunable RunableInstance { get; }
        IFileHandler FileHandler { get; }
        Task InvokeRun();
        bool Load(string rootDirPath);
        Task UnloadContext();
    }
}
