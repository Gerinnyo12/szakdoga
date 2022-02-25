using Service.Interfaces;
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
        private bool _isRunnable = false;

        public Runable(ILogger<Runable> logger)
        {
            _logger = logger;
        }

        public bool CreateInstance(Assembly assembly)
        {
            if (assembly is null)
            {
                _logger.LogError($"A(z) {nameof(assembly)} parameter nem lehet null.");
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

            _logger.LogInformation($"{exportedClass.FullName} sikeresen peldanyositva lett.");
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
                _logger.LogError(ex, $"A futtatando assembly ({assembly.FullName}) nem 1 db publikus osztalyt talalt.");
            }
            return null;
        }

        private bool InitVariables(Assembly assembly, Type exportedClass)
        {
            if (exportedClass.GetInterface("Shared.IWorkerTask") is null)
            {
                _logger.LogError($"A(z) {exportedClass.FullName} osztaly nem implementalja a Shared.IWorkerTask nevu interface-t.");
                return false;
            }
            // Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, and the name of the type.
            var instance = assembly.CreateInstance(exportedClass.FullName);
            var runMethod = exportedClass.GetMethod("Run");
            var timerPropety = exportedClass.GetProperty("Timer");
            var timer = (uint?)timerPropety?.GetValue(instance);
            if (CheckIfPropertyIsNull(instance, runMethod, timer))
            {
                _logger.LogError($"Nem sikerult peldanyositani a(z) {exportedClass.FullName} nevu assemblyt");
                return false;
            }
            _instance = instance;
            _runMethod = runMethod;
            _timer = timer;
            _startedAt = App.IterationCounter + 1;
            _isCurrentlyRunning = false;
            _isRunnable = true;
            return true;
        }

        private bool CheckIfPropertyIsNull(object? instance, MethodInfo? runMethod, uint? timer) =>
            instance == null || runMethod == null || timer == null || timer == 0;

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
                _logger.LogError(ex, $"A(z) {_instance?.GetType()} hibat dobott futas kozben.");
            }
            finally
            {
                _isCurrentlyRunning = false;
            }
        }
        private bool CanStartRun() => (App.IterationCounter - _startedAt) % _timer == 0;

        public void UnleashReferences()
        {
            _isRunnable = false;
            _instance = null;
            _runMethod = null;
            _timer = null;
        }
    }
}
