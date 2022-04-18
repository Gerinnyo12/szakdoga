using Service.Interfaces;
using Shared;
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
        private readonly string _responseForGCCollect = Constants.RESPONSE_JSON_FOR_GC_COLLECT;
        private readonly string _responseForIndefinitRequest = Constants.RESPONSE_JSON_FOR_INDEFINIT_REQUEST;

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
            if (rootDirPath is null) return false;

            using var serviceScope = _serviceScopeFactory.CreateScope();
            IAssemblyContext context = serviceScope.ServiceProvider.GetRequiredService<IAssemblyContext>();
            bool success = context.Load(rootDirPath);

            if (!success)
            {
                _logger.LogError("Nem sikerült betölteni a(z) {zipPath} feladatot.", zipPath);
                context.UnloadContext();
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

            await Task.Run(() =>
            {
                context.UnloadContext();
                Contexts.Remove(zipPath);

                string? rootDirPath = context.GetDirPathOfContext();
                if (rootDirPath is null) return;

                context = null;
                FreeMemoryOfContext(rootDirPath);
                _logger.LogInformation("A(z) {zipPath} sikeresen el lett engedve.", zipPath);
            });

            return true;
        }

        private void FreeMemoryOfContext(string rootDirPath)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            string rootDirName = FileHelper.GetFileName(rootDirPath, withoutExtension: true);
            string localDirPath = FileHelper.GetAbsolutePathOfLocalDir(rootDirName);
            string runnerDirPath = FileHelper.GetAbsolutePathOfRunDir(rootDirName);
            RemoveDir(localDirPath);
            RemoveDir(runnerDirPath);
        }

        private void RemoveDir(string dirPath)
        {
            if (!FileHelper.DirExists(dirPath))
            {
                _logger.LogError("Nem létezik a(z) {dirPath} útvonalú mappa ezért nem is törlődött.", dirPath);
                return;
            }
            try
            {
                FileHelper.DeleteDir(dirPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nem sikerült kitörölni a(z) {dirPath} útvonalú mappát.", dirPath);
            }
        }

        public async Task<string> CreateJsonData(RequestMessage requestMessage)
        {
            switch (requestMessage)
            {
                case RequestMessage.Indefinit:
                    return _responseForIndefinitRequest;
                case RequestMessage.GetDataWithDetails:
                    return await JsonHelper.SerializeAsync(Contexts.Keys);
                case RequestMessage.CallGC:
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    return _responseForGCCollect;
                default:
                    //GetData
                    return await JsonHelper.SerializeAsync(Contexts.Keys
                        .Select(key => FileHelper.GetFileName(key, withoutExtension: true)));
            }
        }
    }
}
