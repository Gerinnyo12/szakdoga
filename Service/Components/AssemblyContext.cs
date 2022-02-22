using Service.Helpers;
using Service.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace Service.Components
{
    public class AssemblyContext : IAssemblyContext
    {
        private readonly string _rootDirectoryPath;
        private readonly string _rootDirectoryName;
        private readonly AssemblyLoadContext _assemblyLoadContext;
        private static readonly Assembly[] _app_domain_default_assemblies;
        public IRunable RunableInstance { get; }

        static AssemblyContext()
        {
            _app_domain_default_assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        // a kicsomagolasi mappa utvonala
        public AssemblyContext(string rootDirectoryPath)
        {
            _rootDirectoryPath = rootDirectoryPath;
            _rootDirectoryName = FileHandler.GetFileName(rootDirectoryPath);
            _assemblyLoadContext = new(null, true);
            RunableInstance = new Runable();
        }

        public bool LoadAssemblies()
        {
            bool isDirCreated = FileHandler.CreateRunnerDirectory(_rootDirectoryName);
            if (!isDirCreated)
            {
                return false;
            }

            string fileName = _rootDirectoryName;
            string? filePath = FileHandler.CheckAndCopyDllToRunnerDir(_rootDirectoryName, fileName);
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
                ////TODO LOGOLNI
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

                string? referencedAssemblyPath = FileHandler.CheckAndCopyDllToRunnerDir(_rootDirectoryName, referencedAssembly.Name);
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
                LogWriter.Log(LogLevel.Error, $"Nem sikerult betolteni a(z) {filePathToLoad} utvonalu assembly-t.");
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
            DeleteFolders();
        }

        private void DeleteFolders()
        {
            string runnerDirectoryPath = FileHandler.GetRunnerDirectory(_rootDirectoryName);
            FileHandler.DeleteDirectoryContent(runnerDirectoryPath, true);
            FileHandler.DeleteDirectoryContent(_rootDirectoryPath, true);
        }
    }
}
