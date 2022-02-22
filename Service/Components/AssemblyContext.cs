using Service.Helpers;
using Service.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace Service.Components
{
    public class AssemblyContext : IAssemblyContext
    {
        private readonly string _rootDirectoryName;
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private readonly IFileHandler _fileHandler;
        private static readonly Assembly[] _app_domain_default_assemblies;
        public IRunable RunableInstance { get; }

        static AssemblyContext()
        {
            _app_domain_default_assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        // a kicsomagolasi mappa tetejenek utvonala
        public AssemblyContext(string rootDirectoryPath)
        {
            _fileHandler = new FileHandler(rootDirectoryPath);
            _rootDirectoryName = FileHelper.GetFileName(rootDirectoryPath);
            _assemblyLoadContext = new(_rootDirectoryName, true);
            RunableInstance = new Runable();
        }

        public bool LoadAssemblies()
        {
            bool isDirCreated = _fileHandler.CreateRunnerDir();
            if (!isDirCreated)
            {
                return false;
            }

            string entryFileName = _rootDirectoryName;
            string? filePath = _fileHandler.CopyDllToRunnerDir(entryFileName);
            if (filePath == null)
            {
                return false;
            }

            Assembly? assembly = HandleLoadWithReferences(filePath);
            if (assembly == null)
            {
                UnloadContext();
                return false;
            }

            bool isInstanceCreated = RunableInstance.CreateInstance(assembly);
            if (!isInstanceCreated)
            {
                UnloadContext();
                return false;
            }
            return true;
        }

        private Assembly? HandleLoadWithReferences(string filePathToLoad)
        {
            // https://social.msdn.microsoft.com/Forums/en-US/caba700d-1011-4c48-8a39-e9513c81baad/delete-dll-wo-closing-the-application?forum=csharplanguage
            // https://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp
            // https://docs.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable
            // https://stackoverflow.com/questions/34549641/async-await-vs-getawaiter-getresult-and-callback

            Assembly? assembly = LoadAssemblyIntoContext(filePathToLoad);
            if (assembly == null)
            {
                return null;
            }

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                //mivel rekurziv a fuggveny, ezert valtozhat 2 iteracio kozott az appdomain
                //ezert mindig ujra ellenorizzuk
                if (_app_domain_default_assemblies.Any(assembly => assembly.FullName == referencedAssembly.FullName))
                {
                    LogWriter.Log(LogLevel.Information, $"A(z) {referencedAssembly.FullName} nevu assembly egy alap dependency, ezert nem kell betolteni");
                    continue;
                }

                if (_assemblyLoadContext.Assemblies.Any(assembly => assembly.FullName == referencedAssembly.FullName))
                {
                    LogWriter.Log(LogLevel.Information, $"A(z) {referencedAssembly.FullName} nevu assembly mar egyszer be van toltve ebbe az AppDomain-be");
                    continue;
                }

                string? referencedAssemblyPath = _fileHandler.CopyDllToRunnerDir(referencedAssembly.Name);
                if (referencedAssemblyPath == null)
                {
                    //nem pontosan 1 ilyen nevu assembly volt a mappaban
                    return null;
                }

                if (HandleLoadWithReferences(referencedAssemblyPath) == null)
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
                Assembly assembly = _assemblyLoadContext.LoadFromAssemblyPath(filePathToLoad);
                return assembly;
            }
            catch (Exception ex)
            {
                LogWriter.Log(LogLevel.Error, $"Nem sikerult betolteni a(z) {filePathToLoad} utvonalu assembly-t: {ex.Message}");
            }
            return null;
        }

        public void UnloadContext()
        {
            RunableInstance.UnleashReferences();
            _assemblyLoadContext.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            string localDirPath = FileHelper.CombinePaths(FileHelper.LocalDir, _rootDirectoryName);
            string runnerDirPath = FileHelper.CombinePaths(FileHelper.RunnerDir, _rootDirectoryName);
            DeleteDir(localDirPath);
            DeleteDir(runnerDirPath);
        }

        private void DeleteDir(string dirPath)
        {
            try
            {
                FileHelper.DeleteDir(dirPath);
            }
            catch (Exception ex)
            {
                LogWriter.Log(LogLevel.Error, $"Nem sikerult kitorolni a(z) {dirPath} utvonalu mappat: {ex.Message}");
            }
        }
    }
}
