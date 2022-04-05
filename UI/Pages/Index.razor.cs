using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared;
using System.ServiceProcess;
using UI.Helpers;
using UI.Models;

namespace UI.Pages
{
    public partial class Index
    {
        private ServiceState _state = ServiceState.Loading;
        private ServiceController? _serviceController;
        private double _secondsToWaitForService;

        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (!firstRender) return;
            try
            {
                if (Constants.IS_WINDOWS)
                {
                    _secondsToWaitForService = Constants.MAX_SECONDS_TO_WAIT_FOR_SERVICE;
                    _serviceController = new(Constants.SERVICE_NAME);
                    SetCurrentState();
                    return;
                }
            }
            catch (Exception ex)
            {
                await JSRuntime.ErrorSwal("A szolgáltatás nem található!", "Hiba történt inicializáláskor!");
            }
            _state = ServiceState.Unavailable;
            StateHasChanged();
        }

        public bool StartService()
        {
            //Win32Eception-t tud dobni, csak nem irja
            //a komponensben elkapom
            _serviceController?.Start();
            _serviceController?.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(_secondsToWaitForService));
            SetCurrentState();
            return _state == ServiceState.Running;
        }

        public bool StopService()
        {
            _serviceController?.Stop();
            _serviceController?.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(_secondsToWaitForService));
            SetCurrentState();
            return _state == ServiceState.Stopped;
        }

        private void SetCurrentState()
        {
            _state = IsServiceRunning() ? ServiceState.Running : ServiceState.Stopped;
            //ez azert kell ide. hogy renderelje ujra a switch-et
            StateHasChanged();
        }

        public bool IsServiceRunning()
        {
            _serviceController?.Refresh();
            return _serviceController?.Status == ServiceControllerStatus.Running
                || _serviceController?.Status == ServiceControllerStatus.StartPending;
        }

        public async Task SuccessAlerter(string message) =>
            await JSRuntime.SuccessAlert("Siker", message);

        public async Task ErrorAlerter(string message) =>
            await JSRuntime.ErrorAlert("Hiba", message);

        public async Task ExceptionAlerter(Exception ex) =>
            await JSRuntime.ErrorAlert("Kivétel dobódott!", $"{ex.Message} {Environment.NewLine} {ex.InnerException}");
    }
}
