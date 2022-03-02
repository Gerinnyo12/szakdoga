namespace Shared.Models
{
    public class AppSettingsJson
    {
        public Logging Logging { get; set; } = new();
        public Parameters Parameters { get; set; } = new();
    }
}
