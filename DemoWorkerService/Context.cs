using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace DemoWorkerService
{
    public class Context
    {
        public AssemblyLoadContext LoadContext { get; }
        public Assembly MainAssembly { get; private set; }
        public Runable Runable { get; set; }

        private readonly string _rootDirectoryPath;
        private readonly string _rootDirectoryName;
        private readonly string _mainFileName;

        // a kicsomagolasi mappa utvonala
        public Context(string rootDirectoryPath)
        {
            _rootDirectoryPath = rootDirectoryPath;
            _rootDirectoryName = Path.GetFileName(_rootDirectoryPath);
            _mainFileName = _rootDirectoryName;
            LoadContext = new(null, true);
            MainAssembly = null;
        }

        public bool LoadAssemblies()
        {
            _ = FileHelper.CreateRunnerDirectory(_rootDirectoryName);

            string filePath = FileHelper.CheckAndCopyDllToRunnerDir(_rootDirectoryName, _mainFileName);
            if (filePath == null)
            {
                return false;
            }

            Assembly assembly = IterateAndLoadAssemblyRecursively(filePath);
            if (assembly == null)
            {
                UnloadContext();
                return false;
            }

            MainAssembly = assembly;
            Runable = new Runable();
            bool isCreated = Runable.CreateRunableInstance(assembly);
            if (!isCreated)
            {
                UnloadContext();
                return false;
            }
            return true;
        }

        private Assembly IterateAndLoadAssemblyRecursively(string filePathToLoad)
        {
            //TODO visszateresi ertek
            // https://social.msdn.microsoft.com/Forums/en-US/caba700d-1011-4c48-8a39-e9513c81baad/delete-dll-wo-closing-the-application?forum=csharplanguage
            // https://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp
            // https://docs.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable

            string assemblyName = Path.GetFileNameWithoutExtension(filePathToLoad);
            // bekerulhet kozben, ezert mindig a frisset kell lekerni
            // TODO EZ IGY CSUNYA
            if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName.StartsWith($"{assemblyName}, ")))
            {
                Assembly assembly = LoadAssemblyIntoContext(filePathToLoad);

                if (assembly == null)
                {
                    return null;
                }

                foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                {
                    string referenceAssemblyPath = FileHelper.CheckAndCopyDllToRunnerDir(_rootDirectoryName, referencedAssembly.Name);
                    if (referenceAssemblyPath == null)
                    {
                        continue;
                    }

                    if (IterateAndLoadAssemblyRecursively(referenceAssemblyPath) == null)
                    {
                        return null;
                    }
                }
                return assembly;
            }
            // https://stackoverflow.com/questions/34549641/async-await-vs-getawaiter-getresult-and-callback
            return null;
        }


        private Assembly LoadAssemblyIntoContext(string filePathToLoad)
        {
            try
            {
                Assembly assembly = LoadContext.LoadFromAssemblyPath(filePathToLoad);
                return assembly;
            }
            catch (Exception ex)
            {
                //TODO LOGOLNI
                Console.WriteLine($"Nem sikerult betolteni a(z) {filePathToLoad} nevu file-t.");
            }
            return null;
        }

        public void UnloadContext()
        {
            Runable.RemoveReferences();
            MainAssembly = null;
            LoadContext.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            string runnerDirectoryPath = FileHelper.GetRunnerDirectory(_rootDirectoryName);
            FileHelper.DeleteDirectoryContent(runnerDirectoryPath, true);
            FileHelper.DeleteDirectoryContent(_rootDirectoryPath, true);
        }
    }
}
