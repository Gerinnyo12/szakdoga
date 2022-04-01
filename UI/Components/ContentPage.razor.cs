using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared;
using Shared.Helpers;
using Shared.Models;
using System.Net.Sockets;

namespace UI.Components
{
    public partial class ContentPage
    {
        [Parameter]
        public Func<bool>? StopService { get; set; }
        [Parameter]
        public Func<bool>? IsServiceStillRunning { get; set; }
        [Parameter]
        public EventCallback SetCurrentState { get; set; }
        [Parameter]
        public EventCallback<string> SuccessAlerter { get; set; }
        [Parameter]
        public EventCallback<string> ErrorAlerter { get; set; }
        [Parameter]
        public EventCallback<Exception> ExceptionAlerter { get; set; }

        private readonly string _hostName = Constants.IP_ADDRESS.ToString();
        private readonly int _port = Constants.PORT;
        private bool _isLoading = false;
        private IEnumerable<string> _runningContexts = Enumerable.Empty<string>();
        private DateTime _lastRefresh;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                await OnRefreshCallback(RequestMessage.GetData);
            }
        }

        private async Task OnRefreshCallback(RequestMessage requestMessage)
        {
            IEnumerable<string>? obj = await HandleRequest<IEnumerable<string>>(requestMessage);
            if (obj is null) return;
            await RefreshUI(obj);
        }

        private async Task<Tout?> HandleRequest<Tout>(RequestMessage requestMessage)
        {
            Tout? obj = await SendRequest<Tout>(requestMessage);
            if (obj is null)
            {
                await ErrorAlerter.InvokeAsync("Nem sikerült frissíteni a UI-t!");
            }
            await CheckIfServiceStillRunning();
            return obj;
        }

        private async Task<Tout?> SendRequest<Tout>(RequestMessage requestMessage)
        {
            try
            {
                TcpClient client = new(_hostName, _port);
                NetworkStream stream = client.GetStream();
                await ListenerHelper.WriteJsonToStream(stream, requestMessage.ToString());
                string response = await ListenerHelper.GetStringFromStream(stream);
                stream.Close();
                client.Close();
                return JsonHelper.Deserialize<Tout>(response);
            }
            catch (Exception ex) { await HandleException(ex); }
            return default;
        }

        private async Task RefreshUI(IEnumerable<string> updatedData)
        {
            _lastRefresh = DateTime.Now;
            _runningContexts = updatedData;
            await SuccessAlerter.InvokeAsync("Az adatok frissültek!");
        }

        private async Task OnStopCallback()
        {
            _isLoading = true;
            await Stop();
            _isLoading = false;
        }

        private async Task Stop()
        {
            await HandleListener();
            try
            {
                if (StopService?.Invoke() ?? false)
                {
                    await SuccessAlerter.InvokeAsync("A szervíz sikeresen leállt!");
                    return;
                }
            }
            catch (Exception ex) { await HandleException(ex); }
            await ErrorAlerter.InvokeAsync("Nem sikerült leállítani a szervízt!");
        }

        private async Task HandleListener()
        {
            if (await SendRequest<string>(RequestMessage.StopListener) == Constants.RESPONSE_STRING_WHEN_LISTENER_STOPS)
            {
                await SuccessAlerter.InvokeAsync("A TCP listener sikeresen leállt!");
                return;
            }
            await ErrorAlerter.InvokeAsync("A TCP listener nem állt le!");
        }

        private async Task HandleException(Exception exception)
        {
            await ExceptionAlerter.InvokeAsync(exception);
            await CheckIfServiceStillRunning();
        }

        private async Task CheckIfServiceStillRunning()
        {
            if (!IsServiceStillRunning?.Invoke() ?? false)
            {
                await ErrorAlerter.InvokeAsync("A művelet során leállt a szervíz!");
                await SetCurrentState.InvokeAsync();
            }
        }

        private async Task UploadFile(InputFileChangeEventArgs files)
        {
            foreach (var file in files.GetMultipleFiles())
            {
                string filePath = FileHelper.GetAbsolutePathOfMonitoredZip(file.Name);
                await using FileStream stream = new(filePath, FileMode.Create);
                await file.OpenReadStream().CopyToAsync(stream);
                await SuccessAlerter.InvokeAsync($"{file.Name} sikeresen átmásolva! A további információk logolva vannak!");
            }
        }

        private async Task RemoveFile(string fileName)
        {
            string filePath = FileHelper.GetAbsolutePathOfMonitoredZip(FileHelper.AppendZipExtensionToFileName(fileName));
            try
            {
                FileHelper.DeleteFile(filePath);
                await SuccessAlerter.InvokeAsync($"{fileName} sikeresen törölve! A további információk logolva vannak!");
            }
            catch (Exception ex) { await ExceptionAlerter.InvokeAsync(ex); }
        }


    }
}
    