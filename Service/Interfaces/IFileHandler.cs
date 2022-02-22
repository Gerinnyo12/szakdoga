namespace Service.Interfaces
{
    public interface IFileHandler
    {
        bool DirectoryExists(string path);
        bool FileExists(string path);
        string? IsFileSingleInFolder(string directoryName, string fileName);
        string? ExtractZipAndGetRootDirPath(string sourceFilePath, string destinationDirectoryName);
        bool IsFileLocked(string filePath);
        string? CheckAndCopyDllToRunnerDir(string directoryName, string fileName);
        bool CreateRunnerDirectory(string directoryName);
        void DeleteDirectoryContent(string directoryPath, bool removePath = false);
        void ClearDirectories();
        string GetRunnerDirectory(string directoryName);
        string GetFileName(string path, bool withoutExtension = false);
    }
}
