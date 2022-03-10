using Service.Helpers;
using Service.Interfaces;

namespace Service.Implementations
{
    public class DllLifter : IDllLifter
    {
        private readonly ILogger<DllLifter> _logger;

        public DllLifter(ILogger<DllLifter> logger) => _logger = logger;

        public bool CreateRunnerDir(string rootDirName)
        {
            if (string.IsNullOrEmpty(rootDirName))
            {
                _logger.LogError("A(z) {nameof(rootDirName)} paraméter se null se üres nem lehet.", nameof(rootDirName));
            }

            string rootDirPath = FileHelper.CombinePaths(FileHelper.RunnerDir, rootDirName);
            try
            {
                FileHelper.CreateDir(rootDirPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nem sikerült a(z) {rootDirPath} nevű mappa létrehozása: {ex.Message}", rootDirPath);
            }
            return false;
        }

        public string? CopyFileToRunnerDir(string rootDirName, string fileNameWithoutExtension)
        {
            if (string.IsNullOrEmpty(rootDirName) || !FileHelper.DirExists(FileHelper.CombinePaths(FileHelper.RunnerDir, rootDirName)))
            {
                _logger.LogError("Először létre kell hozni egy gyökér mappát a CreateRunnerDir függvény segítségével.");
                return null;
            }
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                _logger.LogError("A(z) {nameof(fileNameWithoutExtension)} paraméter se null se üres nem lehet.", nameof(fileNameWithoutExtension));
                return null;
            }

            string dllName = FileHelper.AppendDllExtensionToFileName(fileNameWithoutExtension);
            string? fromFilePath = GetPathOfFileFromDir(rootDirName, dllName);
            if (fromFilePath is null)
            {
                return null;
            }

            string toFilePath = FileHelper.CombinePaths(FileHelper.RunnerDir, rootDirName, dllName);
            try
            {
                FileHelper.CopyFile(fromFilePath, toFilePath);
                return toFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nem sikerült a(z) {dllName} nevű file másolása {fromFilePath}-ból {toFilePath}-ba: {ex.Message}", dllName, fromFilePath, toFilePath, ex.Message);
            }
            return null;
        }

        private string? GetPathOfFileFromDir(string dirName, string dllName)
        {
            try
            {
                string dirPath = FileHelper.CombinePaths(FileHelper.LocalDir, dirName);
                string filePath = FileHelper.GetSingleFile(dirPath, dllName);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pontosan 1 db {dllName} nevű file-nak kell léteznie a {dirName} nevű mappában.", dllName, dirName);
            }
            return null;
        }

    }
}
