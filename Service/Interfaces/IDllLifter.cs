namespace Service.Interfaces
{
    public interface IDllLifter
    {
        bool CreateRunnerDir(string rootDirName);
        string? CopyFileToRunnerDir(string rootDirName, string fileNameWithoutExtension);
    }
}
