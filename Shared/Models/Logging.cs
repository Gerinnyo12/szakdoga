namespace Shared.Models
{
    public class Logging
    {
        public EventLog EventLog { get; set; } = new();
    }
    public class EventLog
    {
        public LogLevel LogLevel { get; set; } =  new();
    }
    public class LogLevel
    {
        private Microsoft.Extensions.Logging.LogLevel _loglevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;
        public string Default
        {
            get { return _loglevel.ToString(); }
            set
            {
                if (!Enum.TryParse(value, out Microsoft.Extensions.Logging.LogLevel _loglevelTmp))
                {
                    throw new ArgumentException($"Nem megfelelő Loglevel");
                }
                _loglevel = _loglevelTmp;
            }
        }
    }
}
