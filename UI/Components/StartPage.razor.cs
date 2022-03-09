using Microsoft.AspNetCore.Components;
using Shared;
using Shared.Helpers;
using Shared.Models;

namespace UI.Components
{
    public partial class StartPage
    {
        private readonly string _appSettingsPath = Constants.APP_SETTINGS_JSON_PATH;
        private bool _isLoading = false;
        private AppSettingsModel Model { get; set; } = new()
        {
            Parameters = new()
            {
                Path = @"C:\Users\reveszg\Desktop\Watched_Folder",
                Pattern = "Asd*",
                MaxCopyTimeInMiliSec = 1000,
            }
        };

        [Parameter]
        public Func<bool>? StartService { get; set; }
        [Parameter]
        public EventCallback<string> SuccessAlerter { get; set; }
        [Parameter]
        public EventCallback<string> ErrorAlerter { get; set; }
        [Parameter]
        public EventCallback<Exception> ExceptionAlerter { get; set; }

        private async Task OnStartCallback()
        {
            _isLoading = true;
            if (!await PassParametersToAppSettings()) return;
            await Start();
            _isLoading = false;
        }

        private async Task<bool> PassParametersToAppSettings()
        {
            try
            {
                var json = await JsonHelper.SerializeAsync(Model);
                File.WriteAllText(_appSettingsPath, json);
                await SuccessAlerter.InvokeAsync("Az appsettings.json sikeresen létre lett hozva!");
                return true;
            }
            catch (Exception ex) { await ExceptionAlerter.InvokeAsync(ex); }
            return false;
        }

        private async Task Start()
        {
            try
            {
                if (StartService?.Invoke() ?? false)
                {
                    await SuccessAlerter.InvokeAsync("A szervíz sikeresen elindult!");
                    return;
                }
            }
            catch (Exception ex) { await ExceptionAlerter.InvokeAsync(ex); }
        }
    }
}
