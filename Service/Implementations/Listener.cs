using Service.Interfaces;
using Shared;
using Shared.Helpers;
using System.Net;
using System.Net.Sockets;

namespace Service.Implementations
{
    public class Listener : IListener
    {
        private readonly IPAddress _ipAddress = Constants.IP_ADDRESS;
        private readonly int _port = Constants.PORT;
        private readonly string _responseWhenListenerStops = Constants.RESPONSE_JSON_WHEN_LISTENER_STOPS;
        private readonly ILogger<Listener> _logger;

        public Listener(ILogger<Listener> logger) => _logger = logger;

        public async Task StartListening(Func<RequestMessage, Task<string>> GetJsonData)
        {
            TcpListener listener = new(_ipAddress, _port);
            listener.Start();
            bool shouldStop = false;
            while (!shouldStop)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    NetworkStream stream = client.GetStream();
                    string data = await ListenerHelper.GetStringFromStream(stream);
                    shouldStop = !Enum.TryParse(data, out RequestMessage requestMessage) || requestMessage == RequestMessage.StopListener;
                    string responseJson = shouldStop ? _responseWhenListenerStops : await GetJsonData(requestMessage);
                    await ListenerHelper.WriteJsonToStream(stream, responseJson);
                    stream.Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hiba tortent a TCP listenerrel!");
                }
            }
            listener.Stop();
            _logger.LogInformation("Sikeresen leallt a listener!");
        }
    }
}
