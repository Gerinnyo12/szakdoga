using Service.Interfaces;
using Shared;
using System.Reflection;

namespace Service.Implementations
{
    public class Runable : IRunable
    {
        private readonly ILogger<Runable> _logger;
        private object? _instance;
        private MethodInfo? _runMethod;
        private uint? _timer;
        private ulong _startedAt;
        private bool _isCurrentlyRunning;
        private bool _isLoaded = false;

        public Runable(ILogger<Runable> logger) => _logger = logger;

        public bool CreateInstance(Assembly assembly)
        {
            if (assembly is null)
            {
                _logger.LogError("A(z) {nameof(assembly)} paraméter se null se üres nem lehet.", nameof(assembly));
                return false;
            }

            var exportedClass = GetExportedClass(assembly);
            if (exportedClass is null) return false;

            bool isInstanceCreated = InitVariables(assembly, exportedClass);
            if (!isInstanceCreated) return false;

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
            (var instance, var runMethod, var timer) = ConstructClassFromAssembly(assembly, exportedClass);
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
            _isLoaded = true;

            return true;
        }

        private (object?, MethodInfo?, uint?) ConstructClassFromAssembly(Assembly assembly, Type exportedClass)
        {
            try
            {
                // Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, and the name of the type.
                var instance = assembly.CreateInstance(exportedClass.FullName);
                var runMethod = exportedClass.GetMethod("Run");
                var timerPropety = exportedClass.GetProperty("Timer");
                var timer = (uint?)timerPropety?.GetValue(instance);

                return (instance, runMethod, timer);
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
            if (!_isLoaded || !CanStartRun()) return;
            // amig nem futott le, addig nem lehet meg egyszer mehivni
            if (_isCurrentlyRunning)
            {
                _logger.LogError("A(z) {_instance?.GetType()} előző futása még nem fejeződött be, " +
                    "ezért most kihagyásra kerül.", _instance?.GetType());
                return;
            }

            try
            {
                _isCurrentlyRunning = true;
                //mivel a runMethod egy Task objektumot ad vissza es ez await-elve van, ezert ez nem is lehet null
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
            _isLoaded = false;
            _instance = null;
            _runMethod = null;
            _timer = null;
        }
    }
}
