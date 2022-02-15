using System.Threading.Tasks;

namespace DemoWorkerService
{
    class Program
    {
        const string MonitoringDirectoryPath = @"C:\Users\reveszg\Desktop\Watched_Folder";
        const string SearchPattern = "*";
        const int WaitDurationInMillisec = 1000;

        static async Task Main(string[] args) =>
            await new App(MonitoringDirectoryPath, SearchPattern, WaitDurationInMillisec).Start();
    }
}
