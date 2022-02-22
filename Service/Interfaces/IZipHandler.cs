namespace Service.Interfaces
{
    public interface IZipHandler
    {
        Task<string?> ExtractZip();
    }
}
