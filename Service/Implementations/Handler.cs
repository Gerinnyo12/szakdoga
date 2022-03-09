using Microsoft.Extensions.Options;
using Service.Interfaces;
using Shared.Models.Parameters;

namespace Service.Implementations
{
    public class Handler : IHandler
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ParametersModel _parameters;
        public IContextContainer ContextContainer { get; }
        public static ulong IterationCounter { get; private set; } = 0;

        public Handler(IContextContainer contextContainer, IOptions<ParametersModel> parameters)
        {
            ContextContainer = contextContainer;
            _parameters = parameters.Value;
            _fileSystemWatcher = new FileSystemWatcher(_parameters.Path, _parameters.Pattern)
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
            foreach (var zipPath in Directory.GetFiles(_parameters.Path, _parameters.Pattern, SearchOption.TopDirectoryOnly))
            {
                OnFileAdd(zipPath);
            }
        }

        private async void OnFileAdd(string zipPath)
        {
            await ContextContainer.LoadAssemblyWithReferences(zipPath, _parameters.MaxCopyTimeInMiliSec);
        }

        private async void OnFileDelete(string zipPath)
        {
            await ContextContainer.FindAndRemoveAssembly(zipPath);
        }
    }
}
