using Shared.Interfaces;

namespace NoDefaultCtor
{
    public class Class1 : IWorkerTask
    {
        public uint Timer => 3;

        public async Task Run()
        {
            
        }

        public Class1(string asd, int qwe)
        {

        }
    }
}