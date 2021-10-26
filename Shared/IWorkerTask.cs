using System.Threading.Tasks;

namespace Shared
{
    public interface IWorkerTask
    {
        uint? Timer { get; set; }
        public Task Run();
    }
}
