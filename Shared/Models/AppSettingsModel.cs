using Shared.Models.Logging;
using Shared.Models.Parameters;

namespace Shared.Models
{
    public class AppSettingsModel
    {
        public LoggingModel Logging { get; set; } = new();
        public ParametersModel Parameters { get; set; } = new();
    }
}
