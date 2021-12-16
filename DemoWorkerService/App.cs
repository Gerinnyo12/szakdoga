using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class App
    {
        private const string LOCAL_DLL_RUNNER_DIR = @"C:\GitRepos\szakdoga\Running_Dlls";
        public static ulong IterationCounter { get; set; } = 0;
        readonly FileSystemWatcher FileSystemWatcher;
        readonly string WatchedDirectory;
        readonly string SearchPattern;
        Dictionary<string, Runable> DLLContainer { get; set; }
        Dictionary<string, LoadContext> Contexts { get; set; }
        Dictionary<string, ulong> ChangeHelper { get; set; }

        public App(string directory, string searchPattern)
        {
            WatchedDirectory = directory;
            SearchPattern = searchPattern;
            FileSystemWatcher = new FileSystemWatcher(WatchedDirectory)
            {
                EnableRaisingEvents = true,
            };
            FileSystemWatcher.Created += (sender, file) => HandleDirectoryAdd(file.FullPath);
            FileSystemWatcher.Deleted += (sender, file) => RemoveDll(file.FullPath);
            FileSystemWatcher.Changed += ReplaceFile;
            DLLContainer = new Dictionary<string, Runable>();
            Contexts = new Dictionary<string, LoadContext>();
            ChangeHelper = new Dictionary<string, ulong>();
            AddAlreadyExistingDlls(directory);
        }

        public async Task Start()
        {
            // 584 554 530 872 evig tud futni
            while(true)
            {
                Console.WriteLine($"{++IterationCounter}. iteracio.");
                //RunDLLs();
                await Task.Delay(1000);
            }
        }

        private async void RunDLLs()
        {
            // https://stackoverflow.com/questions/604831/collection-was-modified-enumeration-operation-may-not-execute
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-5.0

            //Console.WriteLine("Elkezdodott a container futtatasa");
            var dllList = DLLContainer.ToList();
            Parallel.ForEach(dllList, async pair =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                // https://stackoverflow.com/questions/1308432/do-try-catch-blocks-hurt-performance-when-exceptions-are-not-thrown
                string path = pair.Key;
                var runable = pair.Value;
                try
                {
                    if(runable.isCallable)
                    {
                        await runable.InvokeRun();
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"  ELKAPTAM!!!!");
                    //TODO logolni
                }
            });
            //Console.WriteLine("Befejezodott a container futtatasa");
        }
        private void ReplaceFile(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("\t Meghivodott a CHANGE");
            // akkor, ha toroltunk valamit, aztan visszarakjuk nem fut le az add, csak ez
            // mert ez igy csak egy modositas?
            //megkereses, torles es hozzaadas

            if(!ChangeHelper.TryGetValue(e.FullPath, out var value) || value < IterationCounter)
            {
                RemoveDll(e.FullPath);
                HandleDirectoryAdd(e.FullPath);
            }
            else
            {
                Console.WriteLine("\t Nem futott le a change");
            }
        }

        private void AddAlreadyExistingDlls(string watchedDirectory)
        {
            foreach(var directory in Directory.GetDirectories(watchedDirectory))
            {
                HandleDirectoryAdd(directory);
            }
        }

        private void HandleDirectoryAdd(string sourceDirectoryPath)
        {
            string directoryName = Path.GetFileName(sourceDirectoryPath);
            string destinationDirectory = Path.Combine(LOCAL_DLL_RUNNER_DIR, directoryName);
            Directory.CreateDirectory(destinationDirectory);
            try
            {
                //TODO
                string filePath = Directory.GetFiles(sourceDirectoryPath, directoryName + ".dll", SearchOption.AllDirectories).FirstOrDefault();
                if(filePath != null)
                {
                    string fileName = Path.GetFileName(filePath);
                    string destinationFileName = Path.Combine(destinationDirectory, fileName);
                    File.Copy(filePath, destinationFileName, true);
                    AddDLLToContainer(destinationFileName, sourceDirectoryPath);
                }
            }
            //TODO
            catch(Exception e) { }
        }

        private bool AddDLLToContainer(string filePathToLoad, string modifiableFilePath)
        {
            try
            {
                Console.WriteLine("\t Meghivodott az ADD");
                AssemblyLoadContext context = new(null, true);
                Assembly assembly = null;
                bool isLoaded = ReadAssembly(context, ref assembly, filePathToLoad, modifiableFilePath);
                var runable = new Runable();
                bool isCreated = isLoaded && assembly != null && runable.CreateRunableInstance(assembly);
                if(isCreated)
                {
                    var loadContext = new LoadContext(context, assembly);
                    Contexts.Add(modifiableFilePath, loadContext);
                    DLLContainer.Add(modifiableFilePath, runable);
                    ChangeHelper[modifiableFilePath] = IterationCounter + 1;
                    Console.WriteLine($"\t + {Path.GetFileName(filePathToLoad)}");
                    return true;
                }
            }
            catch(Exception ex) { }
            return false;
        }

        private bool ReadAssembly(AssemblyLoadContext loadContext, ref Assembly assemblyToLoadIn, string filePathToLoad, string directoryToSearchFrom)
        {
            //TODO visszateresi ertek
            // https://social.msdn.microsoft.com/Forums/en-US/caba700d-1011-4c48-8a39-e9513c81baad/delete-dll-wo-closing-the-application?forum=csharplanguage
            // https://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp
            // https://docs.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable
            bool done = false;
            int numOfTries = 0;
            // TODO exe handeling
            bool success = true;
            while(!done && numOfTries < 10)
            {
                try
                {
                    // bekerulhet kozben, ezert mindig a frisset kell lekerni
                    if(!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith($"{Path.GetFileNameWithoutExtension(filePathToLoad)}, ")))
                    {
                        if(assemblyToLoadIn == null)
                        {
                            assemblyToLoadIn = loadContext.LoadFromAssemblyPath(filePathToLoad);
                        }
                        success = true;
                        Assembly assembly = null;
                        foreach(var reference in assemblyToLoadIn.GetReferencedAssemblies())
                        {
                            var dllPath = Directory.GetFiles(directoryToSearchFrom, reference.Name + ".dll", SearchOption.AllDirectories).FirstOrDefault();
                            if(dllPath != null)
                            {
                                string fileName = Path.GetFileName(dllPath);
                                string directoryName = Path.GetFileName(directoryToSearchFrom);
                                filePathToLoad = Path.Combine(LOCAL_DLL_RUNNER_DIR, directoryName, fileName);
                                File.Copy(dllPath, filePathToLoad, true);
                                if(!ReadAssembly(loadContext, ref assembly, filePathToLoad, directoryToSearchFrom))
                                {
                                    success = false;
                                }
                            }
                        }
                        assembly = null;
                    }
                    done = true;
                }
                catch(Exception e)
                {
                    success = false;
                    Console.WriteLine($"{numOfTries + 1}. proba");
                    numOfTries++;
                    //Console.WriteLine(e.Message);
                }
                // https://stackoverflow.com/questions/34549641/async-await-vs-getawaiter-getresult-and-callback
            }
            return success;
        }

        private void RemoveDll(string directoryPath)
        {
            // ha ebben benne van, akkor mind a masik kettoben is
            if(Contexts.TryGetValue(directoryPath, out var context))
            {
                try
                {
                    ChangeHelper.Remove(directoryPath);
                    DLLContainer[directoryPath].RemoveReferences();
                    DLLContainer.Remove(directoryPath);
                    context.Assembly = null;
                    Contexts.Remove(directoryPath);
                    context.Context.Unload();
                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    string directoryName = Path.GetFileName(directoryPath);
                    string directoryToDelete = Path.Combine(LOCAL_DLL_RUNNER_DIR, directoryName);
                    Directory.Delete(directoryToDelete, true);
                    Console.WriteLine($"\t - {Path.GetFileName(directoryPath)}");
                }
                //TODO
                catch(Exception ex)
                {
                    Console.WriteLine("\t Ajaj");
                }
            }
            else
            {
                Console.WriteLine("\t NEM VOLT MIT KITOROLNI");
            }

        }
    }
}
