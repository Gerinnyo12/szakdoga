using Microsoft.Extensions.Options;
using Service.Helpers;
using Service.Interfaces;

namespace Service.Implementations
{
    public class Handler : IHandler
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly Params _params;
        public IContextContainer ContextContainer { get; }
        public static ulong IterationCounter { get; private set; } = 0;

        public Handler(IContextContainer contextContainer, IOptions<Params> parameters)
        {
            ContextContainer = contextContainer;
            _params = parameters.Value;
            _fileSystemWatcher = new FileSystemWatcher(_params.Path, _params.Pattern)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName,
            };
            _fileSystemWatcher.Created += (sender, file) => OnFileAdd(file.FullPath);
            _fileSystemWatcher.Deleted += (sender, file) => OnFileDelete(file.FullPath);
            AddExisting();
        }

        public async void RunDlls()
        {
            IterationCounter++;
            await ContextContainer.ExecuteContainer();
        }

        private void AddExisting()
        {
            foreach (var zipPath in Directory.GetFiles(_params.Path, _params.Pattern, SearchOption.TopDirectoryOnly))
            {
                OnFileAdd(zipPath);
            }
        }

        private async void OnFileAdd(string zipPath)
        {
            await ContextContainer.LoadAssemblyWithReferences(zipPath, _params.MaxCopyTimeInMiliSec);
        }

        private async void OnFileDelete(string zipPath)
        {
            await ContextContainer.FindAndRemoveAssembly(zipPath);
        }
    }
}
