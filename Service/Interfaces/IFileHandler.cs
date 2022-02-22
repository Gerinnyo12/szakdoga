namespace Service.Interfaces
{
    public interface IFileHandler
    {
        bool CreateRunnerDir();
        string? CopyDllToRunnerDir(string fileName);
    }
}
