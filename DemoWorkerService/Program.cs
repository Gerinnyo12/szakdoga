using System;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new App( @"C:\Users\reveszg\Desktop\Watched_Folder", "*").Start();
        }
    }
}
