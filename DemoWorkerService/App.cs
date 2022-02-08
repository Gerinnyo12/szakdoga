using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class App
    {
        public static ulong IterationCounter { get; private set; }
        public static int MaxCopyTimeInMiliSec;

        private readonly FileSystemWatcher FileSystemWatcher;
        private readonly string WatchedDirectory;
        private readonly string SearchPattern;
        private Dictionary<string, Context> LoadContexts { get; set; }
        //private Dictionary<string, ulong> ChangeHelper { get; set; }

        public App(string directory, string searchPattern, int maxCopyTimeInMiliSec)
        {
            IterationCounter = 0;
            MaxCopyTimeInMiliSec = maxCopyTimeInMiliSec;
            WatchedDirectory = directory;
            SearchPattern = searchPattern + ".zip";
            FileSystemWatcher = new FileSystemWatcher(directory, searchPattern)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName,
            };
            FileSystemWatcher.Created += (sender, file) => HandleFileInsert(file.FullPath);
            FileSystemWatcher.Deleted += (sender, file) => RemoveFile(file.FullPath);
            //FileSystemWatcher.Changed += ReplaceFile;
            LoadContexts = new();
            //ChangeHelper = new();  //ChangeHelper = new Dictionary<string, ulong>();
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

            var contexts = LoadContexts.ToList();
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

        //private void ReplaceFile(object sender, FileSystemEventArgs e)
        //{
        //    Console.WriteLine("\t Meghivodott a CHANGE");
        //    // akkor, ha toroltunk valamit, aztan visszarakjuk nem fut le az add, csak ez
        //    // mert ez igy csak egy modositas?
        //    //megkereses, torles es hozzaadas

        //    if (!ChangeHelper.TryGetValue(e.FullPath, out var value) || value < IterationCounter)
        //    {
        //        RemoveFile(e.FullPath);
        //        HandleFileInsert(e.FullPath);
        //    }
        //    else
        //    {
        //        Console.WriteLine("\t Nem futott le a change");
        //    }
        //}

        private void AddExisting()
        {
            foreach (var zipPath in Directory.GetFiles(WatchedDirectory, SearchPattern, SearchOption.TopDirectoryOnly))
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

            LoadContexts.Add(zipPath, context);
        }

        private void RemoveFile(string fileToDelete)
        {
            if (!LoadContexts.TryGetValue(fileToDelete, out var context))
            {
                Console.WriteLine("\t NEM VOLT MIT KITOROLNI");
                return;
            }

            context.UnloadContext();
            LoadContexts.Remove(fileToDelete);
            Console.WriteLine($"\t - {Path.GetFileName(fileToDelete)}");
        }

    }
}
