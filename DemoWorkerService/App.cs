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
        public static ulong IterationCounter { get; set; }
        readonly FileSystemWatcher FileSystemWatcher;
        Dictionary<string, Runable> DLLs { get; set; }
        Dictionary<string, ulong> LastModificationOfFiles { get; set; }

        public App()
        {
            FileSystemWatcher = new FileSystemWatcher(@"C:\Users\reveszg\Desktop\DllContainer", "*.dll")
            {
                EnableRaisingEvents = true,
            };
            FileSystemWatcher.Created += DLLAddHandler;
            FileSystemWatcher.Deleted += DLLDeleteHandler;
            FileSystemWatcher.Changed += DLLReplaceHandler;
            DLLs = new Dictionary<string, Runable>();
            LastModificationOfFiles = new Dictionary<string, ulong>();
            AddAlreadyExistingDlls();
            // nem fog elkezdodni az iteralas amig a konstruktor be nem fejezodik
        }

        private void AddAlreadyExistingDlls()
        {
            foreach (var dll in Directory.GetFiles(@"C:\Users\reveszg\Desktop\DllContainer", "*.dll"))
            {
                AddDLLToContainer(dll);
            }
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

            Console.WriteLine("Elkezdodott a container futtatasa");
            var dllList = DLLs.ToList();
            Parallel.ForEach(dllList, async runable =>
            {
                // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                // https://stackoverflow.com/questions/1308432/do-try-catch-blocks-hurt-performance-when-exceptions-are-not-thrown
                try
                {
                    await runable.Value.InvokeRun();
                }
                catch (Exception e)
                {
                    // es ellenorozni, hogy nem toroltuk e mar ki
                    // pl. 1 mp-enkent fut 5mp-ig
                    // tehat altalaban 5x fog futni egyszerre
                    // de ha az elso dob egy hibat, akkor kitoroljuk, de a tobbi 4 mar futasban van
                    // es ezt le kell kezelni
                    Console.WriteLine($"\t{e.Message}\n");
                    if (File.Exists(runable.Key))
                    {
                        File.Delete(runable.Key);
                    }
                }
            });
            Console.WriteLine("Befejezodott a container futtatasa");
        }

        private void DLLReplaceHandler(object sender, FileSystemEventArgs e)
        {
            // megkereses, torles es hozzaadas
            if (LastModificationOfFiles[e.FullPath] < IterationCounter)
            {
                DeleteDLLFromContainer(e.FullPath);
                AddDLLToContainer(e.FullPath);
            }
            else
            {
                Console.WriteLine("Nem futott le a change");
            }
        }

        private void DLLAddHandler(object sender, FileSystemEventArgs e)
        {
            AddDLLToContainer(e.FullPath);
        }

        private void AddDLLToContainer(string path)
        {
            // https://social.msdn.microsoft.com/Forums/en-US/caba700d-1011-4c48-8a39-e9513c81baad/delete-dll-wo-closing-the-application?forum=csharplanguage
            // https://stackoverflow.com/questions/18362368/loading-dlls-at-runtime-in-c-sharp
            byte[] bites = File.ReadAllBytes(path); //TODO eception handling
            var file = Assembly.Load(bites);    //TODO eception handling
            var runable = new Runable();
            bool isCreated = runable.CreateRunableInstance(file);
            if (isCreated)
            {
                DLLs.Add(path, runable);
                Console.WriteLine($"Hozzaadtal egy DLL-t: {path}");
            }
            LastModificationOfFiles.Add(path, IterationCounter + 1);
        }

        private void DLLDeleteHandler(object sender, FileSystemEventArgs e)
        {
            DeleteDLLFromContainer(e.FullPath);
            
        }
        
        private void DeleteDLLFromContainer(string path)
        {
            LastModificationOfFiles.Remove(path);
            if (DLLs.Remove(path)) {
                Console.WriteLine($"Toroltel egy DLL-t: {path}");
            }
            else
            {
                Console.WriteLine("Nem volt mit kitorolni");
            }
        }
    }
}
