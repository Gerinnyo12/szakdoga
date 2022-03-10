using Service.Helpers;
using Service.Interfaces;
using Shared;
using System.Reflection;
using System.Runtime.Loader;

namespace Service.Implementations
{
    public class AssemblyContext : IAssemblyContext
    {
        public IRunable RunableInstance { get; private set; }
        public IDllLifter DllLifter { get; private set; }
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private readonly ILogger<AssemblyContext> _logger;
        private readonly ReferenceHelper _referenceHelper;
        private string? _rootDirPath;

        public AssemblyContext(IRunable runableInstance, IDllLifter dllLifter, ReferenceHelper referenceHelper, ILogger<AssemblyContext> logger)
        {
            _assemblyLoadContext = new AssemblyLoadContext(String.Empty, isCollectible: true);
            RunableInstance = runableInstance;
            DllLifter = dllLifter;
            _referenceHelper = referenceHelper;
            _logger = logger;
        }

        public bool Load(string rootDirPath)
        {
            if (string.IsNullOrEmpty(rootDirPath) || !FileHelper.DirExists(rootDirPath))
            {
                _logger.LogError("A(z) {nameof(rootDirPath)} nem lehet null és léteznie kell az mappának", nameof(rootDirPath));
                return false;
            }

            _rootDirPath = rootDirPath;
            Assembly? mainAssembly = HandleAssemblies();
            if (mainAssembly is null)
            {
                return false;
            }

            bool isInstanceCreated = RunableInstance.CreateInstance(mainAssembly);
            if (!isInstanceCreated)
            {
                return false;
            }
            return true;
        }

        private Assembly? HandleAssemblies()
        {
            string rootDirName = FileHelper.GetFileName(_rootDirPath, withoutExtension: true);
            bool isDirCreated = DllLifter.CreateRunnerDir(rootDirName);
            if (!isDirCreated)
            {
                return null;
            }

            string fileName = rootDirName;
            string? filePath = DllLifter?.CopyFileToRunnerDir(rootDirName, fileName);
            if (filePath is null)
            {
                return null;
            }

            Assembly? assembly = LoadAssembliesWithRecursion(rootDirName, filePath);
            if (assembly is null)
            {
                return null;
            }
            return assembly;
        }

        private Assembly? LoadAssembliesWithRecursion(string rootDirName, string filePathToLoad)
        {
            Assembly? assembly = LoadAssemblyIntoContext(filePathToLoad);
            if (assembly is null)
            {
                return null;
            }

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                if (CanSkipAssembly(referencedAssembly))
                {
                    continue;
                }

                string? referencedAssemblyPath = DllLifter?.CopyFileToRunnerDir(rootDirName, referencedAssembly.Name);
                if (referencedAssemblyPath is null)
                {
                    //nem pontosan 1 ilyen nevu assembly volt a mappaban
                    return null;
                }

                if (LoadAssembliesWithRecursion(rootDirName, referencedAssemblyPath) is null)
                {
                    return null;
                }
            }
            return assembly;
        }

        private Assembly? LoadAssemblyIntoContext(string filePathToLoad)
        {
            try
            {
                Assembly? assembly = _assemblyLoadContext?.LoadFromAssemblyPath(filePathToLoad);
                return assembly;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nem sikerült betölteni a(z) {filePathToLoad} útvonalú assembly-t.", filePathToLoad);
            }
            return null;
        }

        private bool CanSkipAssembly(AssemblyName assemblyName)
        {
            //a Shared.dll azert kivetel mert frissulhet ugy, hogy egy dll meg az elozo verziot hasznalja
            //ilyenkor a regi toltodjon be ne az uj
            if (_referenceHelper.IsAssemblyAlreadyLoaded(assemblyName) && assemblyName.Name != Constants.SHARED_PROJECT_NAME)
            {
                _logger.LogInformation("A(z) {assemblyName.FullName} nevű assembly egy alap dependency, ezert nem kell betölteni", assemblyName.FullName);
                return true;
            }
            else if (_assemblyLoadContext.Assemblies.Any(assembly => assembly.FullName == assemblyName.FullName))
            {
                _logger.LogInformation("A(z) {assemblyName.FullName} nevű assembly már egyszer be van töltve ebbe az AppDomain-be", assemblyName.FullName);
                return true;
            }
            return false;
        }

        public async Task InvokeRun() => await RunableInstance.Run();

        public async Task UnloadContext()
        {
            if (string.IsNullOrEmpty(_rootDirPath) || !FileHelper.DirExists(_rootDirPath))
            {
                _logger.LogError("Ahhoz, hogy ki lehessen törölni, előszor be kell tölteni a context-et a {nameof(Load)} függény segítségével.", nameof(Load));
                return;
            }

            RunableInstance?.UnleashReferences();
            _assemblyLoadContext?.Unload();
            await CallGC();
            HandleDirDelete();
        }

        private async Task CallGC()
        {
            //azert egy kulon task, mert kulonben az UnloadContext nem async futna le
            await Task.Run(() =>
            {
                //azert kell 2x meghivni, mert 2 gc kell ahhoz, hogy kitorlodjon a memoriabol egy assembly objektum
                //es hogy unloadolni lehessen a contextet
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
        }

        private void HandleDirDelete()
        {
            string rootDirName = FileHelper.GetFileName(_rootDirPath, withoutExtension: true);
            string localDirPath = FileHelper.CombinePaths(FileHelper.LocalDir, rootDirName);
            string runnerDirPath = FileHelper.CombinePaths(FileHelper.RunnerDir, rootDirName);
            RemoveDir(localDirPath);
            RemoveDir(runnerDirPath);
        }

        private void RemoveDir(string dirPath)
        {
            if (!FileHelper.DirExists(dirPath))
            {
                _logger.LogError("Nem létezik a(z) {dirPath} útvonalú mappa ezért nem lehet kitörölni.", dirPath);
                return;
            }
            try
            {
                FileHelper.DeleteDir(dirPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nem sikerült kitörölni a(z) {dirPath} útvonalú mappát.", dirPath);
            }
        }

    }
}
