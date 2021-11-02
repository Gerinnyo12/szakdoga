using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Shared;

namespace DemoWorkerService
{
    public class Runable
    {
        // The access level for class members and struct members, including nested classes and structs, is private by default -- Gugli
        object Instance;
        MethodInfo RunMethod;
        uint Timer;
        ulong StartedAt;

        public bool CreateRunableInstance(Assembly dll)
        {
            var runnableClasses = dll.GetExportedTypes();
            if (runnableClasses.Any())
            {
                var runnableClass = runnableClasses.FirstOrDefault();
                if (runnableClass.IsClass && runnableClass.GetInterface(typeof(IWorkerTask).FullName) != null)
                {
                    var instance = Activator.CreateInstance(runnableClass); //TODO exception handling
                    var runMethod = runnableClass.GetMethod("Run");
                    var timerPropety = runnableClass.GetProperty("Timer");
                    uint timer = (uint)timerPropety.GetValue(instance);
                    if (runMethod != null && timer != 0)
                    {
                        Instance = instance;
                        RunMethod = runMethod;
                        Timer = timer;
                        StartedAt = App.IterationCounter + 1;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CanStartRun()
        {
            return (App.IterationCounter - StartedAt) % Timer == 0;
        }

        public async Task<bool> InvokeRun()
        {
            if (CanStartRun())
            {
                await (Task)RunMethod.Invoke(Instance, Array.Empty<object>());
                return true;
            }
            return false;
        }

    }
}
