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
        private readonly FileSystemWatcher FileSystemWatcher;
        private readonly string WatchedDirectory;
        private readonly string SearchPattern;
        private Dictionary<string, Runable> DllContainer { get; set; }
        private Dictionary<string, AssemblyLoadContext> LoadContexts { get; set; }
        private Dictionary<string, ulong> ChangeHelper { get; set; }

        public App(string directory, string searchPattern)
        {
            WatchedDirectory = directory;
            SearchPattern = searchPattern;
            FileSystemWatcher = new FileSystemWatcher(directory, searchPattern)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName,
            };
            FileSystemWatcher.Created += (sender, file) => HandleFileInsert(file.FullPath);
            FileSystemWatcher.Deleted += (sender, file) => RemoveFile(file.FullPath);
            FileSystemWatcher.Changed += ReplaceFile;
            DllContainer = new();  //DLLContainer = new Dictionary<string, Runable>();
            LoadContexts = new();  //Contexts = new Dictionary<string, AssemblyLoadContext>();
            ChangeHelper = new();  //ChangeHelper = new Dictionary<string, ulong>();
            AddAlreadyExistingDlls(directory);
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

            //Console.WriteLine("Elkezdodott a container futtatasa");
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
            //Console.WriteLine("Befejezodott a container futtatasa");
        }
        private void ReplaceFile(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("\t Meghivodott a CHANGE");
            // akkor, ha toroltunk valamit, aztan visszarakjuk nem fut le az add, csak ez
            // mert ez igy csak egy modositas?
            //megkereses, torles es hozzaadas

            if (!ChangeHelper.TryGetValue(e.FullPath, out var value) || value < IterationCounter)
            {
                RemoveFile(e.FullPath);
                HandleFileInsert(e.FullPath);
            }
            else
            {
                Console.WriteLine("\t Nem futott le a change");
            }
        }

        private void AddAlreadyExistingDlls(string watchedDirectory)
        {
            foreach (var directoryPath in Directory.GetDirectories(watchedDirectory))
            {
                string directoryName = Path.GetFileName(directoryPath);
                //TODO SINGLE-LEL,   HIBAT DOB MAGATOL
                string filePath = Directory.GetFiles(directoryPath, "*.dll", SearchOption.AllDirectories).Single(file => Path.GetFileName(file) == directoryName + ".dll");
                if (filePath == null)
                {
                    //TODO LOGOLNI
                    throw new Exception($"A {directoryPath} nevű mappának nincsen {directoryName} nevű dll-je");
                }
                HandleFileInsert(filePath);
            }
        }

        private void HandleFileInsert(string mutableFilePath)
        {
            Console.WriteLine("\t Meghivodott a HANDLE");
            string mutableDirectoryPath = GetRootDirectoryName(mutableFilePath);
            string mutableDirectoryName = Path.GetFileName(mutableDirectoryPath);
            string immutableDirectoryName = Path.Combine(LOCAL_DLL_RUNNER_DIR, mutableDirectoryName);
            Directory.CreateDirectory(immutableDirectoryName);
            try
            {
                //string mutableFilePath = Directory.GetFiles(mutableDirectoryPath, mutableDirectoryName + ".dll", SearchOption.AllDirectories).FirstOrDefault();
                //if (mutableFilePath == null)
                //{
                //    //TODO
                //    Console.WriteLine("\t NINCS MEGFELELŐ NEVŰ FILE");
                //    return;
                //}
                string immutableFileName = Path.GetFileName(mutableFilePath);
                string immutableFilePath = Path.Combine(immutableDirectoryName, immutableFileName);
                File.Copy(mutableFilePath, immutableFilePath, true);
                AddDllToContainer(mutableFilePath, immutableFilePath, mutableDirectoryPath);
            }
            //TODO fail eseten logolni || kitorolni
            catch (Exception ex) { }
        }

        private bool AddDllToContainer(string filePathToWatch, string filePathToLoad, string directoryToSearchFrom)
        {
            try
            {
                Console.WriteLine("\t Meghivodott az ADD");
                AssemblyLoadContext loadContext = new(null, true);
                Assembly assembly = null;
                bool isLoaded = LoadAssembly(loadContext, ref assembly, filePathToLoad, directoryToSearchFrom);
                var runable = new Runable();
                bool isCreated = isLoaded && assembly != null && runable.CreateRunableInstance(assembly);
                if (!isCreated)
                {
                    return false;
                }
                LoadContexts.Add(filePathToWatch, loadContext);
                DllContainer.Add(filePathToWatch, runable);
                ChangeHelper[filePathToWatch] = IterationCounter + 1;
                Console.WriteLine($"\t + {Path.GetFileName(filePathToWatch)}");
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private bool LoadAssembly(AssemblyLoadContext loadContext, ref Assembly assemblyReference, string fileToLoad, string directoryOfFile)
        {
            //TODO visszateresi ertek
            // https://social.msdn.microsoft.com/Forums/en-US/caba700d-1011-4c48-8a39-e9513c81baad/delete-dll-wo-closing-the-application?forum=csharplanguage
            // https://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp
            // https://docs.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable
            bool done = false;
            int numOfTries = 0;
            // TODO exe handeling
            bool success = false;
            while (!done && numOfTries < 10)
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
                        foreach (var reference in assemblyReference.GetReferencedAssemblies())
                        {
                            var referencePath = Directory.GetFiles(directoryOfFile, reference.Name + ".dll", SearchOption.AllDirectories).FirstOrDefault();
                            if (referencePath == null)
                            {
                                continue;
                            }
                            string referenceFileName = Path.GetFileName(referencePath);
                            string directoryName = Path.GetFileName(directoryOfFile);
                            string immutableReferenceFilePath = Path.Combine(LOCAL_DLL_RUNNER_DIR, directoryName, referenceFileName);
                            File.Copy(referencePath, immutableReferenceFilePath, true);
                            if (!LoadAssembly(loadContext, ref assembly, immutableReferenceFilePath, directoryOfFile))
                            {
                                success = false;
                            }
                        }
                    }
                    done = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{numOfTries + 1}. proba");
                    numOfTries++;
                    //Console.WriteLine(e.Message);
                }
                // https://stackoverflow.com/questions/34549641/async-await-vs-getawaiter-getresult-and-callback
            }
            return success;
        }

        private void RemoveFile(string fileToDelete)
        {
            // ha ebben benne van, akkor mind a masik kettoben is
            if (LoadContexts.TryGetValue(fileToDelete, out var loadContext))
            {
                ChangeHelper.Remove(fileToDelete);
                DllContainer[fileToDelete].RemoveReferences();
                DllContainer.Remove(fileToDelete);
                LoadContexts.Remove(fileToDelete);
                loadContext.Unload();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                string directoryName = Path.GetFileNameWithoutExtension(fileToDelete);
                string directoryToDelete = Path.Combine(LOCAL_DLL_RUNNER_DIR, directoryName);
                Directory.Delete(directoryToDelete, true);
                Console.WriteLine($"\t - {Path.GetFileName(fileToDelete)}");
            }
            else
            {
                Console.WriteLine("\t NEM VOLT MIT KITOROLNI");
            }
        }

        private string GetRootDirectoryName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (!Directory.GetDirectories(WatchedDirectory, fileName, SearchOption.TopDirectoryOnly).Any())
            {
                throw new Exception("A hozzáadott file-nak egy ugyan ilyen nevű mappában kell lennie");
            }

            return Path.Combine(WatchedDirectory, fileName);
        }

    }
}
