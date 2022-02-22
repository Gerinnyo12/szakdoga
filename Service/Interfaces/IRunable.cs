using System.Reflection;

namespace Service.Interfaces
{
    public interface IRunable
    {
        bool CreateInstance(Assembly assembly);
        Task<bool> InvokeRun();
        void UnleashReferences();
    }
}
