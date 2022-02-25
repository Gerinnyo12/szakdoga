using Service.Helpers;
using Service.Interfaces;

namespace Service.Implementations
{
    public class FileHandler : IFileHandler
    {
        private readonly ILogger<FileHandler> _logger;

        public FileHandler(ILogger<FileHandler> logger)
        {
            _logger = logger;
        }

        public bool CreateRunnerDir(string rootDirName)
        {
            if (string.IsNullOrEmpty(rootDirName))
            {
                _logger.LogError($"A(z) {nameof(rootDirName)} nem lehet se ures se null.");
            }

            string rootDirPath = FileHelper.CombinePaths(FileHelper.RunnerDir, rootDirName);
            try
            {
                FileHelper.CreateDir(rootDirPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Nem sikerult a(z) {rootDirPath} nevu mappa letrehozasa: {ex.Message}");
            }
            return false;
        }

        public string? CopyFileToRunnerDir(string rootDirName, string fileNameWithoutExtension)
        {
            if (string.IsNullOrEmpty(rootDirName) || !FileHelper.DirExists(FileHelper.CombinePaths(FileHelper.RunnerDir, rootDirName)))
            {
                _logger.LogError("Eloszor letre kell hozni egy gyoker mappat a CreateRunnerDir fuggveny segitsegevel");
                return null;
            }
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                _logger.LogError($"A(z) {nameof(fileNameWithoutExtension)} nem lehet se null se ures");
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
                _logger.LogError(ex, $"Nem sikerult a(z) {dllName} nevu file masolasa {fromFilePath}-bol {toFilePath}-ba: {ex.Message}");
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
                _logger.LogError(ex, $"Pontosan 1 db {dllName} nevű file-nak kell léteznie a {dirName} nevu mappaban.");
            }
            return null;
        }

    }
}
