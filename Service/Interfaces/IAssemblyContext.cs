namespace Service.Interfaces
{
    public interface IAssemblyContext
    {
        IRunable RunableInstance { get; }
        bool LoadAssemblies();
        void UnloadContext();
    }
}
