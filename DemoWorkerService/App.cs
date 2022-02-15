using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class App
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly string _watchedDirectory;
        private readonly string _searchPattern;
        private readonly Dictionary<string, Context> _loadContexts;
        public static ulong IterationCounter { get; private set; }
        public static int MaxCopyTimeInMiliSec { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory">A megfigyelni valo mappa abszolut utvonala.</param>
        /// <param name="searchPattern">
        /// A futtatando .zip file-ok nevenek mintaja.
        /// pl.:  mokus*  azt jelenti, hogy azokat a .zip file-okat figyelje, amik ugy kezdodnek, hogy "mokus", utana lehet barmi, es ugy vegzodnek, hogy .zip.
        /// pl.: mukosoknakHosszuAFarka.zip.
        /// </param>
        /// <param name="maxCopyTimeInMiliSec">
        /// Mennyi milisecet varjon a .zip file-ok masolasanak elkeszuleseig.
        /// Lefele a kovetkezo 2 hatvanyra kerekitodik.
        /// pl. a 8 az 2 + 4 + 8 = 14 milisec-et var.
        /// pl. a 7 az 2 + 4 = 6 milisec-et var.
        /// </param>
        public App(string directory, string searchPattern, int maxCopyTimeInMiliSec)
        {
            IterationCounter = 0;
            MaxCopyTimeInMiliSec = maxCopyTimeInMiliSec;
            _watchedDirectory = directory;
            _searchPattern = searchPattern + ".zip";
            _fileSystemWatcher = new FileSystemWatcher(directory, searchPattern)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName,
            };
            _fileSystemWatcher.Created += (sender, file) => HandleFileInsert(file.FullPath);
            _fileSystemWatcher.Deleted += (sender, file) => RemoveFile(file.FullPath);
            _loadContexts = new();
            FileHelper.ClearDirectories();
            AddExisting();
        }

        public async Task Start()
        {
            // 584 554 530 872 evig tud futni
            while (true)
            {
                Console.WriteLine($"{++IterationCounter}. iteracio.");
                RunDLLs();
                await Task.Delay(1000);
            }
        }

        private async void RunDLLs()
        {
            // https://stackoverflow.com/questions/604831/collection-was-modified-enumeration-operation-may-not-execute
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-5.0

            var contexts = _loadContexts.ToList();
            Parallel.ForEach(contexts, async keyValuePair =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                // https://stackoverflow.com/questions/1308432/do-try-catch-blocks-hurt-performance-when-exceptions-are-not-thrown
                string path = keyValuePair.Key;
                var runable = keyValuePair.Value.Runable;
                if (runable.IsCallable)
                {
                    try
                    {
                        await runable.InvokeRun();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\tELKAPTAM!!!!");
                        Console.WriteLine($"\t{e.InnerException}");
                        //TODO logolni
                    }
                }
            });
        }

        private void AddExisting()
        {
            foreach (var zipPath in Directory.GetFiles(_watchedDirectory, _searchPattern, SearchOption.TopDirectoryOnly))
            {
                HandleFileInsert(zipPath);
            }
        }

        private async void HandleFileInsert(string zipPath)
        {

            var zipHandler = new ZipHelper(zipPath);
            string rootDirectoryPath = await zipHandler.ExtractZip();
            if (rootDirectoryPath == null)
            {
                // vagy tul sok ideig masolodott,
                // vagy nem pont 1 db megfelelo nevu file volt benne
                //TODO LOGOLNI
                Console.WriteLine($"Vagy tul sok ideig masolodott");
                Console.WriteLine($"Vagy nem pont 1 db megfelelo nevu file volt benne");
                return;
            }

            var context = new Context(rootDirectoryPath);
            bool isLoaded = context.LoadAssemblies();
            if (!isLoaded)
            {
                //ha nem sikerult az ossze assembly betoltese
                //TODO LOGOLNI
                Console.WriteLine($"Nem sikerult az ossze assembly betoltese");
                return;
            }

            _loadContexts.Add(zipPath, context);
            Console.WriteLine($"{zipPath} sikeresen hozza lett adva!");
        }

        private void RemoveFile(string fileToDelete)
        {
            if (!_loadContexts.TryGetValue(fileToDelete, out var context))
            {
                Console.WriteLine("\t NEM VOLT MIT KITOROLNI");
                return;
            }

            context.UnloadContext();
            _loadContexts.Remove(fileToDelete);
            Console.WriteLine($"\t - {Path.GetFileName(fileToDelete)}");
        }

    }
}
