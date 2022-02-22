using Service.Helpers;
using Service.Interfaces;

namespace Service.Components
{
    public class App : IApp
    {
        // A TERV AZ H BLAZORBAN KIVALASZTANI A 3 PARAMETERT
        // MAJD START GOMB
        // HOZZABINDOLNI A VALTOZOKHOZ, MAJD EXECUTE

        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly string _watchedDirectory;
        private readonly string _searchPattern;
        private readonly Dictionary<string, IAssemblyContext> _loadContexts;
        private int MaxCopyTimeInMiliSec { get; set; }
        public static ulong IterationCounter { get; private set; }

        public App(string[] args)
        {
            IterationCounter = 0;
            (_watchedDirectory, _searchPattern, MaxCopyTimeInMiliSec) = Arguments.ParseArguments(args);
            _fileSystemWatcher = new FileSystemWatcher(_watchedDirectory, _searchPattern)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName,
            };
            _fileSystemWatcher.Created += (sender, file) => OnFileAdd(file.FullPath);
            _fileSystemWatcher.Deleted += (sender, file) => OnFileDelete(file.FullPath);
            _loadContexts = new();
            AddExisting();
        }

        public async void ExecuteDlls()
        {
            // https://stackoverflow.com/questions/604831/collection-was-modified-enumeration-operation-may-not-execute
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-5.0
            IterationCounter++;
            await Task.Run(() =>
            {
                var contexts = _loadContexts.ToList();
                Parallel.ForEach(contexts, async keyValuePair =>
                {
                    // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                    // https://stackoverflow.com/questions/1308432/do-try-catch-blocks-hurt-performance-when-exceptions-are-not-thrown
                    var runable = keyValuePair.Value.RunableInstance;
                    await runable.InvokeRun();
                });
            });
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
            IZipHandler zipHandler = new ZipHandler(zipPath, MaxCopyTimeInMiliSec);
            string? rootDirPath = await zipHandler.ExtractZip();
            if (rootDirPath == null)
            {
                return;
            }

            IAssemblyContext context = new AssemblyContext(rootDirPath);
            bool isLoaded = context.LoadAssemblies();
            if (!isLoaded)
            {
                return;
            }

            _loadContexts.Add(zipPath, context);
            LogWriter.Log(LogLevel.Information, $"{zipPath} sikeresen hozza lett adva!");
        }

        private void OnFileDelete(string fileToDelete)
        {
            if (!_loadContexts.TryGetValue(fileToDelete, out var context))
            {
                LogWriter.Log(LogLevel.Information, $"A(z) {FileHelper.GetFileName(fileToDelete)} alapbol nem volt hozzaadva.");
                return;
            }

            context.UnloadContext();
            _loadContexts.Remove(fileToDelete);
            LogWriter.Log(LogLevel.Information, $"A(z) {FileHelper.GetFileName(fileToDelete)} sikeresen ki lett torolve.");
        }
    }
}
