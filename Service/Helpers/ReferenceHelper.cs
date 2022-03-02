using System.Reflection;

namespace Service.Helpers
{
    public class ReferenceHelper
    {
        private readonly Assembly[] DefaultAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        public bool IsAssemblyAlreadyLoaded(AssemblyName assemblyName) => 
            DefaultAssemblies.Any(assembly => assembly.FullName == assemblyName.FullName);
    }
}
