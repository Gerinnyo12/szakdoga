using Service.Interfaces;
using System.IO.Compression;

namespace Service.Helpers
{
    public class FileHandler : IFileHandler
    {
        private readonly string _rootDirName;

        public FileHandler(string rootDirPath)
        {
            _rootDirName = FileHelper.GetFileName(rootDirPath, withoutExtension: true);
        }

        public bool CreateRunnerDir()
        {
            string dirPath = FileHelper.CombinePaths(FileHelper.RunnerDir, _rootDirName);
            try
            {
                FileHelper.CreateDir(dirPath);
                return true;
            }
            catch (Exception ex)
            {
                LogWriter.Log(LogLevel.Error, $"Nem sikerult a(z) {dirPath} nevu mappa letrehozasa: {ex.Message}");
            }
            return false;
        }

        public string? CopyDllToRunnerDir(string fileName)
        {
            string dllName = FileHelper.AppendDllExtensionToFileName(fileName);
            string? fromFilePath = GetFilePath(dllName);
            if (fromFilePath == null)
            {
                return null;
            }

            string toFilePath = FileHelper.CombinePaths(FileHelper.RunnerDir, _rootDirName, dllName);
            try
            {
                FileHelper.CopyFile(fromFilePath, toFilePath);
                return toFilePath;
            }
            catch (Exception ex)
            {
                LogWriter.Log(LogLevel.Error, $"Nem sikerult a(z) {dllName} nevu file masolasa {fromFilePath}-bol {toFilePath}-ba: {ex.Message}");
            }
            return null;
        }

        private string? GetFilePath(string dllName)
        {
            try
            {
                string dirPath = FileHelper.CombinePaths(FileHelper.LocalDir, _rootDirName);
                string filePath = FileHelper.GetSingleFile(dirPath, dllName);
                return filePath;
            }
            catch (Exception ex)
            {
                LogWriter.Log(LogLevel.Error, $"Pontosan 1 db {dllName} nevű file-nak kell léteznie a {_rootDirName} nevu mappaban: {ex.Message}");
            }
            return null;
        }

    }
}
