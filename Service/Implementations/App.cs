using Service.Helpers;
using Service.Interfaces;

namespace Service.Implementations
{
    public class App : IApp
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly string _watchedDirectory;
        private readonly string _searchPattern;
        private readonly int _maxCopyTimeInMiliSec;
        public IContextContainer ContextContainer { get; }
        public static ulong IterationCounter { get; private set; } = 0;

        public App(IContextContainer contextContainer, string[] args)
        {
            ContextContainer = contextContainer;
            (_watchedDirectory, _searchPattern, _maxCopyTimeInMiliSec) = Arguments.ParseArguments(args);
            _fileSystemWatcher = new FileSystemWatcher(_watchedDirectory, _searchPattern)
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
            foreach (var zipPath in Directory.GetFiles(_watchedDirectory, _searchPattern, SearchOption.TopDirectoryOnly))
            {
                OnFileAdd(zipPath);
            }
        }

        private async void OnFileAdd(string zipPath)
        {
            await ContextContainer.LoadAssemblyWithReferences(zipPath, _maxCopyTimeInMiliSec);
        }

        private async void OnFileDelete(string zipPath)
        {
            await ContextContainer.FindAndRemoveAssembly(zipPath);
        }
    }
}
