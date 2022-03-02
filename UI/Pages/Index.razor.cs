using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Models;
using System.ServiceProcess;
using System.Text.Json;
using UI.Helpers;

namespace UI.Pages
{
    public partial class Index
    {
        private const string APP_SETTINGS_JSON = "appsettings.json";
        private const string SERVICE_NAME = "Scheduler";
        private const double MAX_SECONDS_UNTIL_SERVICE_STARTS = 30;

        [Inject]
        public IJSRuntime JSRuntime { get; set; }
        private AppSettingsModel Model { get; set; } = new();
        private ServiceController? _serviceController;
        private readonly string _workingDir = Directory.GetCurrentDirectory();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            try
            {
                _serviceController = new(SERVICE_NAME);
            }
            catch (Exception ex)
            {
                await JSRuntime.ErrorSwal("Hiba", $"Nincsen {SERVICE_NAME} nevű szervíz a gépen!");
            }
        }

        private async Task OnStart()
        {
            if (_serviceController is null)
            {
                await JSRuntime.ErrorSwal("Hiba", $"Nincsen {SERVICE_NAME} nevű szervíz a gépen!");
                return;
            }
            //TODO LOADING SPINNER
            var json = JsonSerializer.Serialize(Model);
            string appSettingsPath = Path.Combine(_workingDir, APP_SETTINGS_JSON);
            File.WriteAllText(appSettingsPath, json);
            await StartService();
            //var parameters = JsonSerializer.Deserialize<AppSettingsJson>(json).Parameters;
        }

        private async Task StartService()
        {
            if (_serviceController.Status == ServiceControllerStatus.StartPending || _serviceController.Status == ServiceControllerStatus.Running)
            {
                await JSRuntime.ErrorAlert("Hiba", "A szervíz már fut!");
                return;
            }
            _serviceController.Start();
            await Task.Run(() =>
            {
                _serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(MAX_SECONDS_UNTIL_SERVICE_STARTS));
            });
            await JSRuntime.SuccessAlert("Siker", "A szervíz sikeresen elindult!");
        }

        private async Task OnStop()
        {

        }

    }
}
