using Microsoft.Extensions.Options;
using Service.Interfaces;
using Shared;
using Shared.Helpers;
using Shared.Models.Parameters;

namespace Service.Implementations
{
    public class Handler : IHandler
    {
        public IContextContainer ContextContainer { get; }
        public static ulong IterationCounter { get; private set; } = 0;

        private readonly string _monitoringPath = Constants.MONITORING_DIR_PATH;
        private readonly ParametersModel _parameters;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public Handler(IContextContainer contextContainer, IOptions<ParametersModel> parameters)
        {
            ContextContainer = contextContainer;
            _parameters = parameters.Value;
            _fileSystemWatcher = new FileSystemWatcher(_monitoringPath, _parameters.Pattern)
            {
                EnableRaisingEvents = true,
            };
            _fileSystemWatcher.Created += (sender, file) => OnFileAdd(file.FullPath);
            _fileSystemWatcher.Deleted += (sender, file) => OnFileDelete(file.FullPath);
            AddExisting();
        }
        private void AddExisting() =>
            FileHelper.EnumerateFilesInDir(_monitoringPath, _parameters.Pattern, OnFileAdd);

        public async void RunDlls()
        {
            IterationCounter++;
            await ContextContainer.ExecuteContainer();
        }

        private async void OnFileAdd(string zipPath) =>
            await ContextContainer.LoadAssemblyWithReferences(zipPath, _parameters.MaxCopyTimeInMiliSec);

        private async void OnFileDelete(string zipPath) =>
            await ContextContainer.FindAndRemoveAssembly(zipPath);
    }
}
