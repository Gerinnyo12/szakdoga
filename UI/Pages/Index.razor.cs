using Shared.Models;
using System.Text.Json;

namespace UI.Pages
{
    public partial class Index
    {
        private string Path { get; set; }
        private string Pattern { get; set; }
        private string MaxCopyTimeInMiliSec { get; set; }
        private readonly ServiceController serviceController = new();

        public async Task OnStart()
        {
            if (!ParamsValid())
            {
                return;
            }
            await StartService();
        }

        private bool ParamsValid()
        {
            return true;
        }

        private async Task StartService()
        {
            var settings = new AppSettingsJson()
            {
                Parameters = new()
                {
                    Path = Path,
                    Pattern = Pattern,
                    MaxCopyTimeInMiliSec = MaxCopyTimeInMiliSec,
                }
            };
            var json = JsonSerializer.Serialize<AppSettingsJson>(new AppSettingsJson());
            var parameters = JsonSerializer.Deserialize<AppSettingsJson>(json).Parameters;
        }

    }
}
