using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class Runable
    {
        // The access level for class members and struct members, including nested classes and structs, is private by default -- Gugli
        private object Instance;
        private MethodInfo RunMethod;
        private uint Timer;
        private ulong StartedAt;
        public bool IsCallable { get; private set; }

        public bool CreateRunableInstance(Assembly assembly)
        {
            //minden bele egy try-ba, mert mindig dobhat kivetelt
            try
            {
                var runnableClass = assembly.ExportedTypes.Single();
                if (runnableClass.GetInterface("Shared.IWorkerTask") == null)
                {
                    //TODO LOGOLNI
                    Console.WriteLine($"A(z) {assembly.FullName} nevu assembly nem implementalja a Shared.IWorkerTask nevu interface-t.");
                    return false;
                }
                // itt mar be van toltve az appdomain-be, mert ez a Assembly dll parameter onnan van
                // Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, and the name of the type.
                var instance = assembly.CreateInstance(runnableClass.FullName);
                var runMethod = runnableClass.GetMethod("Run");
                var timerPropety = runnableClass.GetProperty("Timer");
                uint timer = (uint)timerPropety.GetValue(instance);
                if (runMethod == null || timer == 0)
                {
                    return false;
                }
                Instance = instance;
                RunMethod = runMethod;
                Timer = timer;
                StartedAt = App.IterationCounter + 1;
                IsCallable = true;
                return true;
            }
            catch (Exception ex)
            {
                //TODO LOGOLNI
                Console.WriteLine($"Valami nem stimmelt a(z) {assembly.FullName} nevu assembly peldanyositasakor");
            }
            return false;
        }

        private bool CanStartRun()
        {
            return (App.IterationCounter - StartedAt) % Timer == 0;
        }

        public async Task<bool> InvokeRun()
        {
            if (!CanStartRun())
            {
                return false;
            }
            IsCallable = false;
            // amig nem futott le, addig nem lehet megegyszer mehivni
            await (Task)RunMethod.Invoke(Instance, Array.Empty<object>());
            IsCallable = true;
            return true;
        }

        public void RemoveReferences()
        {
            IsCallable = false;
            Instance = null;
            RunMethod = null;
        }

    }
}
