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
                var runnableClass = runnableClasses[0];
                if (runnableClass.IsClass && runnableClass.GetInterface(typeof(IWorkerTask).FullName) != null)
                {
                    var instance = Activator.CreateInstance(runnableClass); //TODO exception handling
                    var runMethod = runnableClass.GetMethod("Run");
                    var timerPropety = runnableClass.GetProperty("Timer");
                    uint? timer = timerPropety.GetValue(instance) as uint?;
                    if (runMethod != null && timer.HasValue)
                    {
                        Instance = instance;
                        RunMethod = runMethod;
                        Timer = timer.Value;
                        StartedAt = App.IterationCounter + 1;
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task InvokeRun()
        {
            Console.WriteLine("Elkezdodottt a Run meghivasa");
            if ((App.IterationCounter - StartedAt) % Timer == 0)
            {
                await (Task)RunMethod.Invoke(Instance, Array.Empty<object>());
            }
            Console.WriteLine("Befejezodott a Run meghivasa");
        }
    }
}
