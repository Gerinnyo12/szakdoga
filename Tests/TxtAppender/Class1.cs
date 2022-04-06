using Shared.Interfaces;

namespace TxtAppender
{
    public class Class1 : IWorkerTask
    {
        public uint Timer { get; set; } = 4;

        public async Task Run()
        {
            await Task.Delay(-1);
            //string path = @"C:\GitRepos\szakdoga\Tests\text.txt";
            //if (!File.Exists(path))
            //{
            //    File.Create(path).Dispose();
            //}
            //using var textAppender = File.AppendText(path);
            //await textAppender.WriteAsync($"Lefutott a TxtAppender projekt Run metódusa {DateTime.Now}-kor!\n");
            //await textAppender.FlushAsync();
        }
    }
}