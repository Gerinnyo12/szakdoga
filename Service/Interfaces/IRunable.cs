using System.Reflection;

namespace Service.Interfaces
{
    public interface IRunable
    {
        bool CreateInstance(Assembly assembly);
        Task Run();
        void UnleashReferences();
    }
}
