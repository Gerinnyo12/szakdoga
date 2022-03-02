namespace Service.Interfaces
{
    public interface IZipExtracter
    {
        Task<string?> ExtractZip(string zipPath, int maxCopyTimeInMiliSec);
    }
}
