using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class Runable
    {
        // The access level for class members and struct members, including nested classes and structs, is private by default -- Gugli
        object Instance;
        MethodInfo RunMethod;
        uint Timer;
        ulong StartedAt;
        public bool isCallable { get; private set; }

        public bool CreateRunableInstance(Assembly assembly)
        {
            var runnableClass = assembly.ExportedTypes.FirstOrDefault();
            if(runnableClass != null && runnableClass.GetInterface("Shared.IWorkerTask") != null)
            {
                try
                {
                    // itt mar be van toltve az appdomain-be, mert ez a Assembly dll parameter onnan van
                    // Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, and the name of the type.
                    var instance = assembly.CreateInstance(runnableClass.FullName);
                    var runMethod = runnableClass.GetMethod("Run");
                    var timerPropety = runnableClass.GetProperty("Timer");
                    uint timer = (uint)timerPropety.GetValue(instance);
                    if(runMethod != null && timer != 0)
                    {
                        Instance = instance;
                        RunMethod = runMethod;
                        Timer = timer;
                        StartedAt = App.IterationCounter + 1;
                        isCallable = true;
                        return true;
                    }
                }
                catch(Exception e)
                {
                    //TODO vmi komolyabb exception handeling
                }
            }
            return false;
        }

        private bool CanStartRun()
        {
            return (App.IterationCounter - StartedAt) % Timer == 0;
        }

        public async Task<bool> InvokeRun()
        {
            if(CanStartRun())
            {
                isCallable = false;
                // amig nem futott le, addig nem lehet megegyszer mehivni
                await (Task)RunMethod.Invoke(Instance, Array.Empty<object>());
                isCallable = true;
                return true;
            }
            return false;
        }

        public void RemoveReferences()
        {
            isCallable = false;
            Instance = null;
            RunMethod = null;
        }

    }
}
