using Service.Interfaces;
using Shared;
using System.Reflection;

namespace Service.Implementations
{
    public class Runable : IRunable
    {
        public Runable(ILogger<Runable> logger) => _logger = logger;

        private readonly ILogger<Runable> _logger;
        private object? _instance;
        private MethodInfo? _runMethod;
        private uint? _timer;
        private ulong _startedAt;
        private bool _isCurrentlyRunning;
        private bool _isRunnable = false;

        public bool CreateInstance(Assembly assembly)
        {
            if (assembly is null)
            {
                _logger.LogError("A(z) {nameof(assembly)} paraméter se null se üres nem lehet.", nameof(assembly));
                return false;
            }

            var exportedClass = GetExportedClass(assembly);
            if (exportedClass is null)
            {
                return false;
            }

            bool isInstanceCreated = InitVariables(assembly, exportedClass);
            if (!isInstanceCreated)
            {
                return false;
            }

            _logger.LogInformation("A(z) {exportedClass.FullName} sikeresen példányosítva lett.", exportedClass.FullName);
            return true;
        }

        private Type? GetExportedClass(Assembly assembly)
        {
            try
            {
                return assembly.ExportedTypes.Single();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A futtatandó ({assembly.FullName}) assembly-ben 1 db publikus osztálynak kell léteznie.", assembly.FullName);
            }
            return null;
        }

        private bool InitVariables(Assembly assembly, Type exportedClass)
        {
            if (exportedClass.GetInterface(Constants.I_WORKER_TASK) is null)
            {
                _logger.LogError("A(z) {exportedClass.FullName} osztály nem implementálja az {Constants.I_WORKER_TASK} nevű interface-t.", exportedClass.FullName, Constants.I_WORKER_TASK);
                return false;
            }
            (var instance, var runMethod, var timeProperty, var timer) = ConstructClassFromAssembly(assembly, exportedClass);
            if (CheckIfPropertiesAreNull(instance, runMethod, timer))
            {
                _logger.LogError("Nem sikerült példányosítani a(z) {exportedClass.FullName} nevű osztályt", exportedClass.FullName);
                return false;
            }
            _instance = instance;
            _runMethod = runMethod;
            _timer = timer;
            _startedAt = Handler.IterationCounter + 1;
            _isCurrentlyRunning = false;
            _isRunnable = true;
            return true;
        }

        private (object?, MethodInfo?, PropertyInfo?, uint?) ConstructClassFromAssembly(Assembly assembly, Type exportedClass)
        {
            try
            {
                // Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, and the name of the type.
                var instance = assembly.CreateInstance(exportedClass.FullName);
                var runMethod = exportedClass.GetMethod("Run");
                var timerPropety = exportedClass.GetProperty("Timer");
                var timer = (uint?)timerPropety?.GetValue(instance);
                return (instance, runMethod, timerPropety, timer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hiba történt a(z) {exportedClass.FullName} példányosítása során!", exportedClass.FullName);
            }
            return default;
        }

        private bool CheckIfPropertiesAreNull(object? instance, MethodInfo? runMethod, uint? timer) =>
            instance is null || runMethod is null || timer is null || timer == 0;

        public async Task Run()
        {
            // amig nem futott le, addig nem lehet megegyszer mehivni
            if (!_isRunnable || !CanStartRun() || _isCurrentlyRunning) return;

            try
            {
                _isCurrentlyRunning = true;
                //mivel a runMethod egy Task objektumot ad vissza ezert ez nem is lehet null
                await (Task)_runMethod?.Invoke(_instance, Array.Empty<object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A(z) {_instance?.GetType()} hibát dobott futás közben.", _instance?.GetType());
            }
            finally
            {
                _isCurrentlyRunning = false;
            }
        }
        private bool CanStartRun() => (Handler.IterationCounter - _startedAt) % _timer == 0;

        public void UnleashReferences()
        {
            _isRunnable = false;
            _instance = null;
            _runMethod = null;
            _timer = null;
        }
    }
}
