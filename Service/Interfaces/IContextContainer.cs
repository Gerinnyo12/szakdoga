namespace Service.Interfaces
{
    public interface IContextContainer
    {
        Dictionary<string, IAssemblyContext> Contexts { get; }
        IZipHandler ZipHandler { get; }
        Task<bool> LoadAssemblyWithReferences(string zipPath, int maxCopyTimeInMiliSec);
        Task ExecuteContainer();
        Task<bool> FindAndRemoveAssembly(string zipPath);
    }
}
