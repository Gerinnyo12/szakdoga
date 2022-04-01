using Shared.Models;

namespace Service.Interfaces
{
    public interface IListener
    {
        void StartListening(Func<RequestMessage, Task<string>> GetJsonData);
    }
}
