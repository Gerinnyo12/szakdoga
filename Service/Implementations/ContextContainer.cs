using Service.Helpers;
using Service.Interfaces;
using Shared.Helpers;

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
            // a file az nyilvan letezik mert azert hivodott meg a callback
            if (string.IsNullOrEmpty(zipPath))
            {
                _logger.LogError("A(z) {nameof(zipPath)} parameter nem lehet null vagy ures.", nameof(zipPath));
                return false;
            }

            string? rootDirPath = await ZipExtracter.ExtractZip(zipPath, maxCopyTimeInMiliSec);
            if (rootDirPath is null)
            {
                _logger.LogError("Nem sikerult a {zipPath} kicsomagolasa", zipPath);
                return false;
            }

            using var serviceScope = _serviceScopeFactory.CreateScope();
            IAssemblyContext context = serviceScope.ServiceProvider.GetRequiredService<IAssemblyContext>();
            bool success = context.Load(rootDirPath);
            if (!success)
            {
                _logger.LogError($"Nem sikerult betolteni a(z) {zipPath} projektet.");
                await context.UnloadContext();
                return false;
            }

            Contexts.Add(zipPath, context);
            _logger.LogInformation("{zipPath} sikeresen be lett toltve!", zipPath);
            return true;
        }

        public async Task ExecuteContainer()
        {
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
                _logger.LogInformation($"A(z) {zipPath} alapbol se volt betoltve.");
                return false;
            }

            await context.UnloadContext();
            Contexts.Remove(zipPath);
            _logger.LogInformation($"A(z) {zipPath} sikeresen el lett engedve es ki lett torolve.");
            return true;
        }

        public async Task<string> CreateJsonData(RequestMessage requestMessage)
        {
            return await JsonHelper.SerializeAsync(requestMessage == RequestMessage.GetData 
                ? Contexts.Keys.Select(key => FileHelper.GetFileName(key, withoutExtension: true)) 
                : Contexts.Keys);
        }
    }
}
