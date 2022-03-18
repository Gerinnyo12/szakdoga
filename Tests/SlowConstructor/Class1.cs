using Shared.Interfaces;

namespace SlowConstructor
{
    public class Class1 : IWorkerTask
    {
        public uint Timer => 5;

        public async Task Run()
        {
            
        }

        public Class1()
        {
            Thread.Sleep(10000);
        }
    }
}