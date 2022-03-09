using Microsoft.AspNetCore.Components;
using Shared;
using Shared.Helpers;
using System.Net.Sockets;

namespace UI.Components
{
    public partial class ContentPage
    {
        private readonly string _hostName = Constants.IP_ADDRESS.ToString();
        private readonly int _port = Constants.PORT;
        private bool _isLoading = false;
        private IEnumerable<string> _runningContexts = Enumerable.Empty<string>();
        private DateTime _lastRefresh;
        [Parameter]
        public Func<bool>? StopService { get; set; }
        [Parameter]
        public Func<bool>? IsServiceStillRunning { get; set; }
        [Parameter]
        public EventCallback<string> SuccessAlerter { get; set; }
        [Parameter]
        public EventCallback<string> ErrorAlerter { get; set; }
        [Parameter]
        public EventCallback<Exception> ExceptionAlerter { get; set; }

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
            if (!IsServiceStillRunning?.Invoke() ?? false)
            {
                await ErrorAlerter.InvokeAsync("A frissítés során leállt a szervíz!");
            }
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
            catch (Exception ex) { await ExceptionAlerter.InvokeAsync(ex); }
            return default;
        }

        private async Task RefreshUI(object obj)
        {
            _lastRefresh = DateTime.Now;
            _runningContexts = obj as IEnumerable<string> ?? Enumerable.Empty<string>();
            await SuccessAlerter.InvokeAsync("Sikeres frissítés!");
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
            catch (Exception ex) { await ExceptionAlerter.InvokeAsync(ex); }
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

    }
}
