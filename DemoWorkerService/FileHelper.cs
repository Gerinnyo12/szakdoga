using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DemoWorkerService
{
    public class FileHelper
    {
        public const string LOCAL_DIR = @"C:\GitRepos\szakdoga\Local";
        public const string RUNNER_DIR = @"C:\GitRepos\szakdoga\Runner";

        public static string ExtractZipAndGetRootDirPath(string sourceFilePath, string destinationDirectoryName)
        {
            // https://stackoverflow.com/questions/10982104/wait-until-file-is-completely-written
            string destinationDirectoryPath = Path.Combine(LOCAL_DIR, destinationDirectoryName);
            // EZ MERGELI NEM PEDIG FELULIRJA
            try
            {
                ZipFile.ExtractToDirectory(sourceFilePath, destinationDirectoryPath, true);
                return destinationDirectoryPath;
            }
            catch (Exception ex)
            {
                //TODO LOGOLNI
                Console.WriteLine("Nem sikerult a masolas");
            }
            return null;
        }

        /// <summary>
        /// Megnezi, hogy a mappaban ilyen nevu dll-bol pontosan 1 db van-e.
        /// </summary>
        /// <param name="directoryName">A mappa, amiben keres</param>
        /// <param name="fileName">A file neve, amit keres (.dll kiterjesztes nelkul)</param>
        /// <returns>A dll file abszolut utvonalat, vagy null-t, ha nem pontosan 1 van.</returns>
        public static string GetSingleFileInFolder(string directoryName, string fileName)
        {
            try
            {
                string directoryPath = Path.Combine(LOCAL_DIR, directoryName);
                string dllName = AppendDllExtensionToFileName(fileName);
                string filePath = Directory.GetFiles(directoryPath, dllName, SearchOption.AllDirectories).Single();
                return filePath;
            }
            catch (Exception ex)
            {
                //TODO LOG
                Console.WriteLine($"Pontosan 1 db {fileName} nevű file-nak kell léteznie a {directoryName} nevu mappaban");
            }
            return null;
        }

        public static void DeleteDirectoryContent(string directoryPath, bool removePath = false)
        {
            // erre azert van szukseg, mert
            // egy .zip kocsomagolasa ossze mergeli a mar ott levo file-okkal
            //https://stackoverflow.com/questions/1288718/how-to-delete-all-files-and-folders-in-a-directory
            DirectoryInfo rootDirectory = new DirectoryInfo(directoryPath);
            try
            {
                foreach (DirectoryInfo directory in rootDirectory.EnumerateDirectories())
                {
                    directory.Delete(true);
                }
                if (removePath)
                {
                    Directory.Delete(directoryPath, true);
                }
            }
            catch (Exception ex)
            {
                //TODO LOGOLNI
                Console.WriteLine($"Valszeg mar nem letezik az {directoryPath} utvonalu mappa.");
            }

        }

        public static string GetRunnerDirectory(string directoryName) =>
            Path.Combine(RUNNER_DIR, directoryName);

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    Console.WriteLine("MEGERKEZETT A ZIP");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\t MEG NEM ERKEZETT MEG A ZIP");
            }
            return true;
        }

        public static string CreateRunnerDirectory(string directoryName)
        {
            string directoryPath = GetRunnerDirectory(directoryName);
            Directory.CreateDirectory(directoryPath);
            return directoryPath;
        }

        /// <summary>
        /// Megnezi, hogy pontosan 1 ilyen nevu .dll van-e a mappaban, es a local-bol a runner-be masolja azt.
        /// </summary>
        /// <param name="directoryName"></param>
        /// <param name="fileName"></param>
        /// <returns>A masolt file helye, vagy null, ha nem pontosan 1 ilyen nevu .dll volt.</returns>
        public static string CheckAndCopyDllToRunnerDir(string directoryName, string fileName)
        {
            string sourceFilePath = GetSingleFileInFolder(directoryName, fileName);
            if (sourceFilePath == null)
            {
                return null;
            }

            string dllName = AppendDllExtensionToFileName(fileName);
            string destinationFilePath = Path.Combine(RUNNER_DIR, directoryName, dllName);
            try
            {
                File.Copy(sourceFilePath, destinationFilePath, true);
                return destinationFilePath;
            }
            catch (Exception ex)
            {
                //TODO LOGOLNI
                Console.WriteLine("Nem sikerult a masolas :(");
            }
            return null;
        }

        private static string AppendDllExtensionToFileName(string fileName) =>
            fileName + ".dll";

        /// <summary>
        /// A kitorli a ket lokalis mappa tartalmat
        /// </summary>
        public static void ClearDirectories()
        {
            DeleteDirectoryContent(LOCAL_DIR);
            DeleteDirectoryContent(RUNNER_DIR);
        }

        public static string GetFileName(string path, bool withoutExtension = false) =>
            withoutExtension ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);
    }
}
