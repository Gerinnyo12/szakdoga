using Shared;
using System.Reflection;

namespace Service.Helpers
{
    public class ReferenceHelper
    {
        //valahol mashol kell inicializalni a DefaultAssemblies-t, hogy ne legyen erre a fuggvenyre referenciaja
        //ezert kell egy masik osztalyban lennie egy static member-kent, mert
        //es igy tud mukodni egy kind of lazy loading
        //itt semmi se lehet static, mert azt a GC nem tudja eltakaritani, es nem lehetne elengedni az assembly-ket
        public bool IsAssemblyAlreadyLoaded(AssemblyName assemblyName)
        {
            //a Shared.dll azert kivetel mert frissulhet ugy, hogy egy dll meg az elozo verziot hasznalja
            //ilyenkor a regi toltodjon be es ne az ujat hasznalja
            return PreLoadedAssemblies.DefaultAssemblies?
                .Any(assembly => assembly.FullName == assemblyName.FullName 
                && assemblyName.Name != Constants.SHARED_PROJECT_NAME) 
                ?? false;
        }

    }
}
