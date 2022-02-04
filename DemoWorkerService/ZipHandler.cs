using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class ZipHandler
    {
        private const string RUNNER_DIR = @"C:\GitRepos\szakdoga\Runner";
        private const string LOCAL_DIR = @"C:\GitRepos\szakdoga\Local";

        // a .zip utonala a monitorozott mappaban
        private readonly string _zipPath;

        // a futtatas helye
        public string RunnerFolder { get; }

        // a kicsomagolas helye
        private string _extractactionDirectoryPath;


        private string _extractactionDirectoryName;

        private string _dllName;

        private readonly int _maxCopyTimeInMiliSec;

        public ZipHandler(string zipPath, int MaxCopyTimeInMiliSec)
        {
            _zipPath = zipPath;
            _extractactionDirectoryName = Path.GetFileNameWithoutExtension(zipPath);
            _dllName = _extractactionDirectoryName + ".dll";
            _maxCopyTimeInMiliSec = MaxCopyTimeInMiliSec;
        }

        public async Task<bool> ExtractZip()
        {
            if (!await AwaitFileLock())
            {
                return false;
            }

            _extractactionDirectoryPath = ExtractZipAndGetRootDirPath();
            if (!SatisfiesRequirements())
            {
                DeleteDirectoryContent(_extractactionDirectoryPath, true);
                return false;
            }

            string filePath = Directory.GetFiles(rootDirectory, _dllName, SearchOption.AllDirectories).Single();
            string runningDirectoryPath = Path.Combine(RUNNER_DIR, directoryName);
            Directory.CreateDirectory(runningDirectoryPath);

            string runningFilePath = Path.Combine(runningDirectoryPath, fileName);
            File.Copy(filePath, runningFilePath, true);

            return true;
        }



        private async Task<bool> AwaitFileLock()
        {
            bool isFileLocked = true;
            int delayCounter = 2;
            while (delayCounter <= _maxCopyTimeInMiliSec && isFileLocked)
            {
                await Task.Delay(delayCounter);
                isFileLocked = IsFileLocked();
                delayCounter *= 2;
            }
            if (isFileLocked)
            {
                //TODO LOG 
                Console.WriteLine($"{_zipPath} nem volt elérhető {delayCounter} milisec után sem.");
                return false;
            }
            return true;
        }

        private bool IsFileLocked()
        {
            try
            {
                using (FileStream stream = File.Open(_zipPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    Console.WriteLine("BELEMENT A USING-BA");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\t LEFUTOTT A CATCH AG");
            }
            return true;
        }

        private string ExtractZipAndGetRootDirPath()
        {
            // https://stackoverflow.com/questions/10982104/wait-until-file-is-completely-written
            string destinationDirectoryPath = Path.Combine(LOCAL_DIR, _extractactionDirectoryName);
            // EZ MERGELI NEM PEDIG FELULIRJA
            ZipFile.ExtractToDirectory(_zipPath, destinationDirectoryPath, true);
            return destinationDirectoryPath;
        }

        private bool SatisfiesRequirements()
        {
            try
            {
                Directory.GetFiles(_extractactionDirectoryPath, _dllName, SearchOption.AllDirectories).Single();
                return true;
            }
            catch (Exception ex)
            {
                //TODO LOG
                Console.WriteLine($"Pontosan 1 db {_dllName} nevű file-nak kell léteznie");
            }
            return false;
        }

        public static void ClearDirectories()
        {
            DeleteDirectoryContent(LOCAL_DIR);
            DeleteDirectoryContent(RUNNER_DIR);
        }

        private static void DeleteDirectoryContent(string directoryPath, bool deleteWrapperDir = false)
        {
            // erre azert van szukseg, mert
            // egy .zip kocsomagolasa ossze mergeli a mar ott levo file-okkal
            //https://stackoverflow.com/questions/1288718/how-to-delete-all-files-and-folders-in-a-directory
            DirectoryInfo rootDirectory = new DirectoryInfo(directoryPath);
            foreach (DirectoryInfo directory in rootDirectory.EnumerateDirectories())
            {
                directory.Delete(true);
            }
            if (deleteWrapperDir)
            {
                Directory.Delete(directoryPath, true);
            }
        }


    }
}
