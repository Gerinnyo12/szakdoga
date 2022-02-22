namespace Service.Interfaces
{
    public interface IZipExtractor
    {
        Task<string?> ExtractZip();
    }
}
