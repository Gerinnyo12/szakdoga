using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
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
        public bool isCallable { get; set; }

        public bool CreateRunableInstance(Assembly assembly)
        {
            var runnableClass = assembly.ExportedTypes.FirstOrDefault();
            if(runnableClass != null && runnableClass.GetInterface("Shared.IWorkerTask") != null)
            {
                try
                {
                    // itt mar be van toltve az appdomain-be, mert ez a Assembly dll parameter onnan van
                    // Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, and the name of the type.
                    var asd = AppDomain.CurrentDomain.GetAssemblies();
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

        public bool CanStartRun()
        {
            return (App.IterationCounter - StartedAt) % Timer == 0;
        }

        public async Task<bool> InvokeRun()
        {
            if(CanStartRun())
            {
                await (Task)RunMethod.Invoke(Instance, Array.Empty<object>());
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
