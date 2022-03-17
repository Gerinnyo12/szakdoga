using Shared.Models;

namespace Service.Interfaces
{
    public interface IContextContainer
    {
        Dictionary<string, IAssemblyContext> Contexts { get; }
        IZipExtracter ZipExtracter { get; }
        Task<bool> LoadAssemblyWithReferences(string zipPath, int maxCopyTimeInMiliSec);
        Task ExecuteContainer();
        Task<bool> FindAndRemoveAssembly(string zipPath);
        Task<string> CreateJsonData(RequestMessage requestMessage);
    }
}
