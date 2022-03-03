using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared;
using Shared.Models;
using System.ServiceProcess;
using UI.Helpers;

namespace UI.Pages
{
    public enum ServiceState
    {
        Loading,
        Running,
        Stopped,
        Unavailable,
    }

    public partial class Index
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        private AppSettingsModel Model { get; set; } = new();
        private string _appSettingsPath;
        private string _serviceName;
        private double _maxSecondsUntilServiceStarts;
        private ServiceState _state = ServiceState.Stopped;
        private ServiceController? _serviceController;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            try
            {
                _appSettingsPath = Constants.APP_SETTINGS_JSON_PATH;
                _serviceName = Constants.SERVICE_NAME;
                _maxSecondsUntilServiceStarts = Constants.MAX_SECONDS_UNTIL_SERVICE_STARTS;
            }
            catch (Exception ex)
            {
                _state = ServiceState.Unavailable;
                return;
            }
            //itt a kontroller mar nem lehet null mert le van kezelve ha nem letezne a szerviz
            _serviceController = new(_serviceName);
            if (_serviceController.Status == ServiceControllerStatus.Running || _serviceController.Status == ServiceControllerStatus.StartPending)
            {
                _state = ServiceState.Running;
            }
        }

        private async Task OnStart()
        {
            var json = JsonConverter.Serialize(Model);
            File.WriteAllText(_appSettingsPath, json);
            await StartService();
        }

        private async Task StartService()
        {
            _serviceController?.Start();
            _state = ServiceState.Loading;
            await Task.Run(() =>
            {
                _serviceController?.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(_maxSecondsUntilServiceStarts));
            });
            _state = ServiceState.Running;
            await JSRuntime.SuccessAlert("Siker", "A szervíz sikeresen elindult!");
        }

        private async Task OnStop()
        {
            _serviceController?.Stop();
            _state = ServiceState.Loading;
            await Task.Run(() =>
            {
                _serviceController?.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(_maxSecondsUntilServiceStarts));
            });
            _state = ServiceState.Stopped;
            await JSRuntime.SuccessAlert("Siker", "A szervíz sikeresen leállítva!");
        }

        private async Task OnRefresh()
        {
            //TODO
        }

    }
}
