using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class App
    {
        public static ulong IterationCounter { get; set; } = 0;
        readonly FileSystemWatcher FileSystemWatcher;
        Dictionary<string, Runable> DLLContainer { get; set; }
        Dictionary<string, ulong> ChangeHelper { get; set; }

        public App(string directory, string searchPattern)
        {
            FileSystemWatcher = new FileSystemWatcher(directory, searchPattern)
            {
                EnableRaisingEvents = true,
            };
            FileSystemWatcher.Created += (sender, file) => AddDLL(file.FullPath);
            FileSystemWatcher.Deleted += (sender, file) => DeleteDLLFromContainer(file.FullPath);
            FileSystemWatcher.Changed += ReplaceFile;
            DLLContainer = new Dictionary<string, Runable>();
            ChangeHelper = new Dictionary<string, ulong>();
            AddAlreadyExistingDlls(directory, searchPattern);
        }

        public async Task Start()
        {
            // 584 554 530 872 evig tud futni
            while (true)
            {
                Console.WriteLine($"{++IterationCounter}. iteracio.");
                RunDLLs();
                await Task.Delay(1000);
            }
        }

        private async void RunDLLs()
        {
            // https://stackoverflow.com/questions/604831/collection-was-modified-enumeration-operation-may-not-execute
            // https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-5.0

            //Console.WriteLine("Elkezdodott a container futtatasa");
            var dllList = DLLContainer.ToList();
            Parallel.ForEach(dllList, async pair =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                // https://stackoverflow.com/questions/1308432/do-try-catch-blocks-hurt-performance-when-exceptions-are-not-thrown
                string path = pair.Key;
                var runable = pair.Value;
                try
                {
                    await runable.InvokeRun();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"  ELKAPTAM!!!!");
                    //ha meg nem toroltuk a fajlt akkor toroljuk
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            });
            //Console.WriteLine("Befejezodott a container futtatasa");
        }

        private void AddAlreadyExistingDlls(string directory, string searchPattern)
        {
            foreach (var dll in Directory.GetFiles(directory, searchPattern))
            {
                AddDLL(dll);
            }
        }

        private void ReplaceFile(object sender, FileSystemEventArgs e)
        {
            // megkereses, torles es hozzaadas
            if (ChangeHelper[e.FullPath] < IterationCounter)
            {
                DeleteDLLFromContainer(e.FullPath);
                AddDLL(e.FullPath);
            }
            else
            {
                Console.WriteLine("\t Nem futott le a change");
            }
        }

        private void AddDLL(string path)
        {
            AddDLLToContainer(path);
            ChangeHelper[path] = IterationCounter + 1;
        }

        private async void AddDLLToContainer(string path)
        {
            // azert async, hogy be lehessen varni a task.delay-eket blockolas nelkul
            // https://social.msdn.microsoft.com/Forums/en-US/caba700d-1011-4c48-8a39-e9513c81baad/delete-dll-wo-closing-the-application?forum=csharplanguage
            // https://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp
            byte[] bites = null;
            Assembly file = null;
            bool done = false;
            int numOfTries = 0;
            while (!done && numOfTries <= 10)
            {
                try
                {
                    if (bites == null)
                    {
                        bites = File.ReadAllBytes(path);
                    }
                    if (file == null)
                    {
                        file = Assembly.Load(bites);
                    }
                    done = true;
                }
                catch (Exception e)
                {
                    numOfTries++;
                    Console.WriteLine($"{numOfTries}. proba");
                    //Console.WriteLine(e.Message);
                    await Task.Delay(100);
                }
                // https://stackoverflow.com/questions/34549641/async-await-vs-getawaiter-getresult-and-callback
            }
            var runable = new Runable();
            bool isCreated = file != null && runable.CreateRunableInstance(file);
            if (isCreated)
            {
                DLLContainer.Add(path, runable);
                Console.WriteLine($"\t + {Path.GetFileName(path)}");
            }
        }

        private void DeleteDLLFromContainer(string path)
        {
            ChangeHelper.Remove(path);
            if (DLLContainer.Remove(path))
            {
                Console.WriteLine($"\t - {Path.GetFileName(path)}");
            }
            else
            {
                Console.WriteLine("Nem volt mit kitorolni");
            }
        }
    }
}
