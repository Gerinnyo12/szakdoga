using Service.Interfaces;

namespace Service.Helpers
{
    public class LogWriter : ILogWriter
    {
        private ILogger<ILogWriter>? _logger;

        private static ILogWriter? _instance;

        public LogWriter(ILogger<ILogWriter> logger)
        {
            Console.WriteLine("Lefutott az ctor...");
            //if (_instance == null)
            //{
            //    _instance = new LogWriter(logger);
            //}
        }

        public void WriteLog(LogLevel logLevel, string message)
        {
            if (_logger == null)
            {
                return;
            }
            switch (logLevel)
            {
                case LogLevel.Critical:
                    _logger.LogCritical(message);
                    break;
                case LogLevel.Debug:
                    _logger.LogDebug(message);
                    break;
                case LogLevel.Error:
                    _logger.LogError(message);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(message);
                    break;
                case LogLevel.Trace:
                    _logger.LogTrace(message);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(message);
                    break;
            }
        }

        public static void Log(LogLevel logLevel, string message)
        {
            if (_instance == null)
            {
                return;
            }
            _instance.WriteLog(logLevel, message);
        }
    }
}
