namespace Service.Interfaces
{
    public interface IAssemblyContext
    {
        IRunable RunableInstance { get; }
        IDllLifter DllLifter { get; }
        Task InvokeRun();
        bool Load(string rootDirPath);
        void UnloadContext();
        string? GetDirPathOfContext();
    }
}
