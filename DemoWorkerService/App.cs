using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class App
    {
        public static ulong IterationCounter { get; set; } = 0;
        private readonly FileSystemWatcher FileSystemWatcher;
        private readonly string WatchedDirectory;
        private readonly string SearchPattern;
        private readonly int MaxCopyTimeInMiliSec;
        private Dictionary<string, Runable> DllContainer { get; set; }
        private Dictionary<string, AssemblyLoadContext> LoadContexts { get; set; }
        //private Dictionary<string, ulong> ChangeHelper { get; set; }

        public App(string directory, string searchPattern, int maxCopyTimeInMiliSec)
        {
            WatchedDirectory = directory;
            SearchPattern = searchPattern + ".zip";
            MaxCopyTimeInMiliSec = maxCopyTimeInMiliSec;
            FileSystemWatcher = new FileSystemWatcher(directory, searchPattern)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName,
            };
            FileSystemWatcher.Created += (sender, file) => HandleFileInsert(file.FullPath);
            FileSystemWatcher.Deleted += (sender, file) => RemoveFile(file.FullPath);
            //FileSystemWatcher.Changed += ReplaceFile;
            DllContainer = new();  //DLLContainer = new Dictionary<string, Runable>();
            LoadContexts = new();  //Contexts = new Dictionary<string, AssemblyLoadContext>();
            //ChangeHelper = new();  //ChangeHelper = new Dictionary<string, ulong>();
            ZipHandler.ClearDirectories();
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

            var dllList = DllContainer.ToList();
            Parallel.ForEach(dllList, async pair =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                // https://stackoverflow.com/questions/1308432/do-try-catch-blocks-hurt-performance-when-exceptions-are-not-thrown
                string path = pair.Key;
                var runable = pair.Value;
                if (runable.isCallable)
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
            try
            {
                Console.WriteLine("\t Meghivodott a HANDLE");



                //TODO KULO METODUSBA
                //!!!
                //TODO REFAKTOR CLASS-OKBA (ODA METODUSOKAT)
                //TODO REFAKTOR METODUSOKBA

                string rootDirectory = ExtractZipAndGetRootDir(zipPath);
                string directoryName = Path.GetFileName(rootDirectory);
                string fileName = directoryName + ".dll";
                //LE VAN KEZELVE AZ EXTRACT-BAN
                string filePath = Directory.GetFiles(rootDirectory, fileName, SearchOption.AllDirectories).Single();
                string immutableDirectory = Path.Combine(RUNNER_DIR, directoryName);

                Directory.CreateDirectory(immutableDirectory);
                string targetFilePath = Path.Combine(immutableDirectory, fileName);
                File.Copy(filePath, targetFilePath, true);
                AddDllToContainer(targetFilePath, rootDirectory, zipPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //TODO fail eseten logolni || kitorolni
            }
        }

        private bool AddDllToContainer(string fileToLoad, string directoryToSearchFrom, string monitoringFileName)
        {
            Console.WriteLine("\t Meghivodott az ADD");
            AssemblyLoadContext loadContext = new(null, true);
            Assembly assembly = null;
            bool isLoaded = LoadAssembly(loadContext, ref assembly, fileToLoad, directoryToSearchFrom);
            var runable = new Runable();
            bool isCreated = isLoaded && assembly != null && runable.CreateRunableInstance(assembly);
            if (!isCreated)
            {
                return false;
            }
            LoadContexts.Add(monitoringFileName, loadContext);
            DllContainer.Add(monitoringFileName, runable);
            //ChangeHelper[triggerFileName] = IterationCounter + 1;
            Console.WriteLine($"\t + {Path.GetFileName(monitoringFileName)}");
            return true;
        }

        private bool LoadAssembly(AssemblyLoadContext loadContext, ref Assembly assemblyReference, string fileToLoad, string rootDirectory)
        {
            //TODO visszateresi ertek
            // https://social.msdn.microsoft.com/Forums/en-US/caba700d-1011-4c48-8a39-e9513c81baad/delete-dll-wo-closing-the-application?forum=csharplanguage
            // https://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp
            // https://docs.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable
            bool done = false;
            bool success = false;
            // TODO exe handeling
            for (int i = 0; i < 10 && !done; i++)
            {
                try
                {
                    string assemblyName = Path.GetFileNameWithoutExtension(fileToLoad);
                    // bekerulhet kozben, ezert mindig a frisset kell lekerni
                    if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith($"{assemblyName}, ")))
                    {
                        if (assemblyReference == null)
                        {
                            assemblyReference = loadContext.LoadFromAssemblyPath(fileToLoad);
                        }
                        success = true;
                        Assembly assembly = null;
                        foreach (var referencedAssembly in assemblyReference.GetReferencedAssemblies())
                        {
                            //TODO SINGLE
                            var assemblyFilePath = Directory.GetFiles(rootDirectory, referencedAssembly.Name + ".dll", SearchOption.AllDirectories).FirstOrDefault();
                            if (assemblyFilePath == null)
                            {
                                continue;
                            }
                            string assemblyFileName = Path.GetFileName(assemblyFilePath);
                            string assemblyDirectoryName = Path.GetFileName(rootDirectory);
                            string targetFilePath = Path.Combine(RUNNER_DIR, assemblyDirectoryName, assemblyFileName);
                            File.Copy(assemblyFilePath, targetFilePath, true);
                            if (!LoadAssembly(loadContext, ref assembly, targetFilePath, rootDirectory))
                            {
                                success = false;
                            }
                        }
                    }
                    done = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{i + 1}. proba");
                    //Console.WriteLine(e.Message);
                }
                // https://stackoverflow.com/questions/34549641/async-await-vs-getawaiter-getresult-and-callback
            }
            return success;
        }

        private void RemoveFile(string fileToDelete)
        {
            // ha ebben benne van, akkor mind a masik kettoben is
            if (!LoadContexts.TryGetValue(fileToDelete, out var loadContext))
            {
                Console.WriteLine("\t NEM VOLT MIT KITOROLNI");
                return;
            }
            //ChangeHelper.Remove(fileToDelete);
            DllContainer[fileToDelete].RemoveReferences();
            DllContainer.Remove(fileToDelete);
            LoadContexts.Remove(fileToDelete);
            loadContext.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            string directoryName = Path.GetFileNameWithoutExtension(fileToDelete);
            string localDirToDelete = Path.Combine(LOCAL_DIR, directoryName);
            string runnerDirToDelete = Path.Combine(RUNNER_DIR, directoryName);
            Directory.Delete(localDirToDelete, true);
            Directory.Delete(runnerDirToDelete, true);
            Console.WriteLine($"\t - {Path.GetFileName(fileToDelete)}");
        }

    }
}
