using System.IO.Compression;

namespace Service.Helpers
{
    public class FileHelper
    {
        private static readonly string _localDir;
        private static readonly string _runnerDir;

        static FileHelper()
        {
            string currentDir = Directory.GetCurrentDirectory();
            _localDir = Path.Combine(currentDir, "Local");
            if (!Directory.Exists(_localDir))
            {
                Directory.CreateDirectory(_localDir);
            }
            _runnerDir = Path.Combine(currentDir, "Runner");
            if (!Directory.Exists(_runnerDir))
            {
                Directory.CreateDirectory(_runnerDir);
            }
        }

        /// <summary>
        /// Megnezi, hogy a mappaban ilyen nevu dll-bol pontosan 1 db van-e.
        /// </summary>
        /// <param name="directoryName">A mappa, amiben keres</param>
        /// <param name="fileName">A file neve, amit keres (.dll kiterjesztes nelkul)</param>
        /// <returns>A dll file abszolut utvonalat, vagy null-t, ha nem pontosan 1 van.</returns>
        public static string? GetSingleFileInFolder(string directoryName, string fileName)
        {
            try
            {
                string directoryPath = Path.Combine(_localDir, directoryName);
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

        public static string? ExtractZipAndGetRootDirPath(string sourceFilePath, string destinationDirectoryName)
        {
            // https://stackoverflow.com/questions/10982104/wait-until-file-is-completely-written
            string destinationDirectoryPath = Path.Combine(_localDir, destinationDirectoryName);
            // EZ MERGELI NEM PEDIG FELULIRJA
            try
            {
                ZipFile.ExtractToDirectory(sourceFilePath, destinationDirectoryPath, true);
                return destinationDirectoryPath;
            }
            catch (Exception ex)
            {
                //TODO LOGOLNI
                Console.WriteLine("Nem sikerult a kicsomagolas");
            }
            return null;
        }

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                Console.WriteLine("MEGERKEZETT A ZIP");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\t MEG NEM ERKEZETT MEG A ZIP");
            }
            return true;
        }

        /// <summary>
        /// Megnezi, hogy pontosan 1 ilyen nevu .dll van-e a mappaban, es a local-bol a runner-be masolja azt.
        /// </summary>
        /// <param name="directoryName"></param>
        /// <param name="fileName"></param>
        /// <returns>A masolt file helye, vagy null, ha nem pontosan 1 ilyen nevu .dll volt.</returns>
        public static string? CheckAndCopyDllToRunnerDir(string directoryName, string fileName)
        {
            string? sourceFilePath = GetSingleFileInFolder(directoryName, fileName);
            if (sourceFilePath == null)
            {
                return null;
            }

            string dllName = AppendDllExtensionToFileName(fileName);
            string destinationFilePath = Path.Combine(_runnerDir, directoryName, dllName);
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

        public static string? CreateRunnerDirectory(string directoryName)
        {
            string directoryPath = GetRunnerDirectory(directoryName);
            Directory.CreateDirectory(directoryPath);
            return directoryPath;
        }

        public static void DeleteDirectoryContent(string directoryPath, bool removePath = false)
        {
            // erre azert van szukseg, mert
            // egy .zip kocsomagolasa ossze mergeli a mar ott levo file-okkal
            //https://stackoverflow.com/questions/1288718/how-to-delete-all-files-and-folders-in-a-directory
            DirectoryInfo rootDirectory = new(directoryPath);
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

        /// <summary>
        /// A kitorli a ket lokalis mappa tartalmat
        /// </summary>
        public static void ClearDirectories()
        {
            DeleteDirectoryContent(_localDir);
            DeleteDirectoryContent(_runnerDir);
        }

        public static string GetRunnerDirectory(string directoryName) =>
            Path.Combine(_runnerDir, directoryName);

        public static string GetFileName(string path, bool withoutExtension = false) =>
            withoutExtension ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);

        private static string AppendDllExtensionToFileName(string fileName) =>
            fileName + ".dll";
    }
}
