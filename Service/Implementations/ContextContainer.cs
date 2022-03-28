using Service.Interfaces;
using Shared.Helpers;
using Shared.Models;

namespace Service.Implementations
{
    public class ContextContainer : IContextContainer
    {
        public Dictionary<string, IAssemblyContext> Contexts { get; private set; }
        public IZipExtracter ZipExtracter { get; private set; }

        private readonly IListener _listener;
        private readonly ILogger<ContextContainer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ContextContainer(IZipExtracter zipExtracter, IListener listener, IServiceScopeFactory serviceScopeFactory, ILogger<ContextContainer> logger)
        {
            Contexts = new Dictionary<string, IAssemblyContext>();
            ZipExtracter = zipExtracter;
            _listener = listener;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _listener.StartListening(CreateJsonData);
        }

        public async Task<bool> LoadAssemblyWithReferences(string zipPath, int maxCopyTimeInMiliSec)
        {
            if (string.IsNullOrEmpty(zipPath))
            {
                _logger.LogError("A(z) {nameof(zipPath)} paraméter se null se üres nem lehet.", nameof(zipPath));
                return false;
            }
            if (!FileHelper.FileExists(zipPath))
            {
                _logger.LogError("Nem létezik a(z) {zipPath} útvonalú file!", zipPath);
                return false;
            }

            string? rootDirPath = await ZipExtracter.ExtractZip(zipPath, maxCopyTimeInMiliSec);
            if (rootDirPath is null)
            {
                return false;
            }

            using var serviceScope = _serviceScopeFactory.CreateScope();
            IAssemblyContext context = serviceScope.ServiceProvider.GetRequiredService<IAssemblyContext>();
            bool success = context.Load(rootDirPath);
            if (!success)
            {
                _logger.LogError("Nem sikerült betölteni a(z) {zipPath} projektet.", zipPath);
                await context.UnloadContext();
                return false;
            }

            Contexts.Add(zipPath, context);
            _logger.LogInformation("{zipPath} sikeresen be lett töltve!", zipPath);
            return true;
        }

        public async Task ExecuteContainer()
        {
            //ilyenkor fog tovabbmenni a motor
            await Task.Run(() =>
            {
                var contexts = Contexts.ToList();
                Parallel.ForEach(contexts, async context =>
                {
                    await context.Value.InvokeRun();
                });
            });
        }

        public async Task<bool> FindAndRemoveAssembly(string zipPath)
        {
            if (!Contexts.TryGetValue(zipPath, out var context))
            {
                _logger.LogInformation("A(z) {zipPath} alapból se volt betöltve, ezért nem is törlődik ki.", zipPath);
                return false;
            }

            await context.UnloadContext();
            Contexts.Remove(zipPath);
            _logger.LogInformation("A(z) {zipPath} sikeresen el lett engedve és ki lett törölve.", zipPath);
            return true;
            //ez utan a kovetkezo GC elviszi a context objektumot
        }

        public async Task<string> CreateJsonData(RequestMessage requestMessage)
        {
            if (requestMessage == RequestMessage.Indefinit)
            {
                //TODO
                return "Ejjejj, egy RequestMessage enum értéket adj at a listenernek...";
            }

            return await JsonHelper.SerializeAsync(requestMessage == RequestMessage.GetData
                ? Contexts.Keys.Select(key => FileHelper.GetFileName(key, withoutExtension: true))
                : Contexts.Keys);
        }
    }
}
