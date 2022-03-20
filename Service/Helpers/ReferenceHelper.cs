using Shared;
using System.Reflection;

namespace Service.Helpers
{
    public class ReferenceHelper
    {
        private Assembly[]? DefaultAssemblies;

        public void InitDefaultAssemblies()
        {
            if (DefaultAssemblies is null)
            {
                DefaultAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
        }

        //es igy tud mukodni egy kind of lazy loading
        //itt semmi se lehet static, mert azt a GC nem tudja eltakaritani, nem lehetne elengedni az assembly-ket
        public bool IsAssemblyAlreadyLoaded(AssemblyName assemblyName)
        {
            //a Shared.dll azert kivetel mert frissulhet ugy, hogy egy dll meg az elozo verziot hasznalja
            //ilyenkor a regi toltodjon be es ne az ujat hasznalja
            return DefaultAssemblies?
                .Any(assembly => assembly.FullName == assemblyName.FullName 
                && assemblyName.Name != Constants.SHARED_PROJECT_NAME) 
                ?? false;
        }

    }
}
