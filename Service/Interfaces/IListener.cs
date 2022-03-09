using Shared.Helpers;

namespace Service.Interfaces
{
    public interface IListener
    {
        Task StartListening(Func<RequestMessage, Task<string>> GetJsonData);
    }
}
