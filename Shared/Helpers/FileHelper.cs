using System.IO.Compression;

namespace Shared.Helpers
{
    public static class FileHelper
    {
        private static string MonitoringDir { get; } = Constants.MONITORING_DIR_PATH;
        private static string LocalDir { get; } = Constants.LOCAL_DIR_PATH;
        private static string RunDir { get; } = Constants.RUNNER_DIR_PATH;
        private static string DLL_EXTENSION = Constants.DLL_EXTENSION;
        private static string ZIP_EXTENSION = Constants.ZIP_EXTENSION;

        public static void EnumerateFilesInDir(string dirPath, string pattern, Action<string> action) =>
            Directory.EnumerateFiles(dirPath, pattern, SearchOption.TopDirectoryOnly)
            .ToList()
            .ForEach(file => action(file));

        public static string GetSingleFile(string rootDir, string filePath) =>
            Directory.GetFiles(rootDir, filePath, SearchOption.AllDirectories).Single();

        public static string GetDirParent(string dirPath) =>
            Directory.GetParent(dirPath)?.FullName ?? string.Empty;

        public static DirectoryInfo CreateDir(string dirPath) =>
            Directory.CreateDirectory(dirPath);

        public static void DeleteDir(string directoryPath) =>
            Directory.Delete(directoryPath, true);
        public static bool DirExists(string path) => Directory.Exists(path);

        public static FileStream OpenFile(string filePath) =>
            File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        public static bool FileExists(string path) => File.Exists(path);

        public static void WriteTextToFile(string filePath, string text) => 
            File.WriteAllText(filePath, text);

        public static void CopyFile(string sourcePath, string destinationPath) =>
            File.Copy(sourcePath, destinationPath, true);

        public static void DeleteFile(string filePath) =>
            File.Delete(filePath);

        public static void ExtractZip(string zipPath, string destinationPath) =>
            ZipFile.ExtractToDirectory(zipPath, destinationPath, true);

        public static string CombinePaths(params string[] paths) => Path.Combine(paths);

        public static string GetFileName(string path, bool withoutExtension = false) =>
            withoutExtension ? Path.GetFileNameWithoutExtension(path) : Path.GetFileName(path);

        public static string GetAbsolutePathOfMonitoredZip(string zipName) =>
            CombinePaths(MonitoringDir, zipName);

        public static string GetAbsolutePathOfLocalDir(string dirPath) =>
            CombinePaths(LocalDir, dirPath);

        public static string GetAbsolutePathOfRunDir(string dirPath) =>
            CombinePaths(RunDir, dirPath);

        public static string AppendDllExtensionToFileName(string fileName) =>
            fileName + DLL_EXTENSION;

        public static string AppendZipExtensionToFileName(string fileName) =>
            fileName + ZIP_EXTENSION;
    }
}
