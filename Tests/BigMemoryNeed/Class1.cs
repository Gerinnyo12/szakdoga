using Shared.Interfaces;

namespace BigMemoryNeed
{
    public class Class1 : IWorkerTask
    {
        public uint Timer => 3;

        public async Task Run()
        {
            await Task.Run(() =>
            {
                int[] bigArray = new int[2500000];
                for (int i = 0; i < bigArray.Length; i++)
                {
                    bigArray[i] = i;
                }
            });
        }
    }
}