namespace Service.Interfaces
{
    public interface ILogWriter
    {
        void WriteLog(LogLevel logLevel, string message);
    }
}
