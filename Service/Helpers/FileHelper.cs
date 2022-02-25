using System.IO.Compression;

namespace Service.Helpers
{
    public class FileHelper
    {
        private const string LOCAL_DIR_NAME = "Local";
        private const string RUNNER_DIR_NAME = "Runner";
        private const string DLL_EXTENSION = ".dll";
        public static string LocalDir;
        public static string RunnerDir;

        public static void PrepareDirs()
        {
            LocalDir = CreateAndGetDirPath(LOCAL_DIR_NAME);
            RunnerDir = CreateAndGetDirPath(RUNNER_DIR_NAME);
        }

        private static string CreateAndGetDirPath(string dirName)
        {
            string workingDir = GetWorkingDir();
            string path = CombinePaths(workingDir, dirName);
            DeleteDir(path);
            CreateDir(path);
            return path;
        }

        public static string GetWorkingDir() => Directory.GetCurrentDirectory();

        public static bool DirExists(string path) => Directory.Exists(path);

        public static bool FileExists(string path) => File.Exists(path);

        public static string GetSingleFile(string rootDir, string filePath) =>
            Directory.GetFiles(rootDir, filePath, SearchOption.AllDirectories).Single();

        public static string CombinePaths(params string[] paths) => Path.Combine(paths);

        public static void ExtractZip(string zipPath, string destinationPath) =>
            ZipFile.ExtractToDirectory(zipPath, destinationPath, true);

        public static FileStream OpenFile(string filePath) =>
            File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        public static void CopyFile(string sourcePath, string destinationPath) =>
            File.Copy(sourcePath, destinationPath, true);

        public static DirectoryInfo CreateDir(string dirPath) =>
            Directory.CreateDirectory(dirPath);

        public static void DeleteDir(string directoryPath) =>
            Directory.Delete(directoryPath, true);

        public static string GetFileName(string path, bool withoutExtension = false) =>
            withoutExtension ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);

        public static string AppendDllExtensionToFileName(string fileName) =>
            fileName + DLL_EXTENSION;
    }
}
