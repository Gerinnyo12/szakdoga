namespace Shared.Models.Logging
{
    public class LogLevelModel
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
