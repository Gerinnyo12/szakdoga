using Shared.Models;

namespace Service.Interfaces
{
    public interface IListener
    {
        Task StartListening(Func<RequestMessage, Task<string>> GetJsonData);
    }
}
