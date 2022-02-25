namespace Service.Interfaces
{
    public interface IFileHandler
    {
        bool CreateRunnerDir(string rootDirName);
        string? CopyFileToRunnerDir(string rootDirName, string fileNameWithoutExtension);
    }
}
