using Service.Helpers;
using Service.Interfaces;
using System.Reflection;

namespace Service.Components
{
    public class Runable : IRunable
    {
        private object _instance;
        private MethodInfo _runMethod;
        private uint _timer;
        private ulong _startedAt;
        private bool IsCurrentlyRunning { get; set; }

        public bool CreateInstance(Assembly assembly)
        {
            //minden bele egy try-ba, mert mindig dobhat kivetelt
            try
            {
                var runnableClass = assembly.ExportedTypes.Single();
                if (runnableClass.GetInterface("Shared.IWorkerTask") == null)
                {
                    LogWriter.Log(LogLevel.Error, $"A(z) {assembly.FullName} nevu assembly nem implementalja a Shared.IWorkerTask nevu interface-t.");
                    return false;
                }
                // itt mar be van toltve az appdomain-be, mert ez a Assembly dll parameter onnan van
                // Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, and the name of the type.
                var instance = assembly.CreateInstance(runnableClass.FullName);
                var runMethod = runnableClass.GetMethod("Run");
                //ez biztos nem null mert mar volt egy check, hogy imlementalja az interface-t
                var timerPropety = runnableClass.GetProperty("Timer")!;
                var timer = timerPropety != null ? (uint?)timerPropety.GetValue(instance) : 0;
                if (ErrorWithProperties(instance, runMethod, timer))
                {
                    return false;
                }
                _instance = instance!;
                _runMethod = runMethod!;
                _timer = timer!.Value;
                _startedAt = App.IterationCounter + 1;
                IsCurrentlyRunning = false;
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.Log(LogLevel.Error, $"Valami nem stimmelt a(z) {assembly.FullName} nevu assembly peldanyositasakor: {ex.Message}");
            }
            return false;
        }

        private bool ErrorWithProperties(object? instance, MethodInfo? runMethod, uint? timer) =>
            instance == null || runMethod == null || timer == null || timer == 0;

        private bool CanStartRun() => (App.IterationCounter - _startedAt) % _timer == 0;

        public async Task<bool> InvokeRun()
        {
            // amig nem futott le, addig nem lehet megegyszer mehivni
            if (!CanStartRun() || IsCurrentlyRunning)
            {
                return false;
            }

            try
            {
                IsCurrentlyRunning = true;
                //ha nem sikerul a dereferalas akkor az azt jelentoi, hogy exception dobodott
                await (Task)_runMethod.Invoke(_instance, Array.Empty<object>())!;
                return true;
            }
            catch (Exception ex)
                {
                LogWriter.Log(LogLevel.Error, $"A(z) {_instance.GetType()} hibat dobott futas kozben, de el lett kapva: {ex.Message}");
            }
            finally
            {
                IsCurrentlyRunning = false;
            }
            return false;
        }

        public void UnleashReferences()
        {
            IsCurrentlyRunning = false;
            //ez miatt kell nullable-nek lennie
            _instance = null;
            _runMethod = null;
        }

    }
}
