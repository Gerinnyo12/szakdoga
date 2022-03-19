using System.Reflection;

namespace Service.Helpers
{
    public static class PreLoadedAssemblies
    {
        public static Assembly[]? DefaultAssemblies;
        public static void InitDefaultAssemblies()
        {
            if (DefaultAssemblies is null)
            {
                DefaultAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
        }
    }
}
