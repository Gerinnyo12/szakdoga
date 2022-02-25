using Service.Helpers;
using Service.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace Service.Implementations
{
    public class AssemblyContext : IAssemblyContext
    {
        public IRunable RunableInstance { get; private set; }
        public IFileHandler FileHandler { get; private set; }
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AssemblyContext> _logger;
        private readonly Assembly[] DEFAULT_ASSEMBLIES;
        private string? _rootDirPath;

        public AssemblyContext(IRunable runableinstance, IFileHandler fileHandler, IServiceScopeFactory serviceScopeFactory, ILogger<AssemblyContext> logger, Assembly[] defaultAssemblies)
        {
            _assemblyLoadContext = new AssemblyLoadContext(String.Empty, isCollectible: true);
            RunableInstance = runableinstance;
            FileHandler = fileHandler;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            DEFAULT_ASSEMBLIES = defaultAssemblies;
        }

        public bool Load(string rootDirPath)
        {
            if (string.IsNullOrEmpty(rootDirPath) || !FileHelper.DirExists(rootDirPath))
            {
                _logger.LogError($"A(z) {nameof(rootDirPath)} nem lehet null es leteznie kell az mappanak");
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
            bool isDirCreated = FileHandler.CreateRunnerDir(rootDirName);
            if (!isDirCreated)
            {
                return null;
            }

            string fileName = rootDirName;
            string? filePath = FileHandler?.CopyFileToRunnerDir(rootDirName, fileName);
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

                string? referencedAssemblyPath = FileHandler?.CopyFileToRunnerDir(rootDirName, referencedAssembly.Name);
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
                _logger.LogError(ex, $"Nem sikerult betolteni a(z) {filePathToLoad} utvonalu assembly-t.");
            }
            return null;
        }

        private bool CanSkipAssembly(AssemblyName assemblyName)
        {
            if (DEFAULT_ASSEMBLIES.Any(assembly => assembly.FullName == assemblyName.FullName))
            {
                _logger.LogInformation($"A(z) {assemblyName.FullName} nevu assembly egy alap dependency, ezert nem kell betolteni");
                return true;
            }
            else if (_assemblyLoadContext.Assemblies.Any(assembly => assembly.FullName == assemblyName.FullName))
            {
                _logger.LogInformation($"A(z) {assemblyName.FullName} nevu assembly mar egyszer be van toltve ebbe az AppDomain-be");
                return true;
            }
            return false;
        }

        public async Task InvokeRun()
        {
            await RunableInstance.Run();
        }

        public async Task UnloadContext()
        {
            if (string.IsNullOrEmpty(_rootDirPath) || !FileHelper.DirExists(_rootDirPath))
            {
                _logger.LogError($"Ahhoz, hogy ki lehessen torolni, eloszor be kell tolteni a context-et a {nameof(Load)} fuggveny segitsegevel.");
                return;
            }

            RunableInstance?.UnleashReferences();
            _assemblyLoadContext?.Unload();
            await CallGC();

            string rootDirName = FileHelper.GetFileName(_rootDirPath, withoutExtension: true);
            string localDirPath = FileHelper.CombinePaths(FileHelper.LocalDir, rootDirName);
            string runnerDirPath = FileHelper.CombinePaths(FileHelper.RunnerDir, rootDirName);
            RemoveDir(localDirPath);
            RemoveDir(runnerDirPath);
        }

        private async Task CallGC()
        {
            await Task.Run(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
        }

        private void RemoveDir(string dirPath)
        {
            if (!FileHelper.DirExists(dirPath))
            {
                _logger.LogError($"Nem letezik a(z) {dirPath} utvonalu mappa ezert nem lehet kitorolni.");
                return;
            }
            try
            {
                FileHelper.DeleteDir(dirPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Nem sikerult kitorolni a(z) {dirPath} utvonalu mappat.");
            }
        }

    }
}
