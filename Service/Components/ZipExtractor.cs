using Service.Helpers;
using Service.Interfaces;

namespace Service.Components
{
    public class ZipExtractor : IZipExtractor
    {
        // a .zip utvonala a monitorozott mappaban
        private readonly string _zipPath;
        private readonly string _rootDirectoryName;
        private readonly int _maxCopyTimeInMiliSec;

        public ZipExtractor(string zipPath, int maxCopyTimeInMiliSec)
        {
            _zipPath = zipPath;
            _rootDirectoryName = FileHandler.GetFileName(zipPath, withoutExtension: true);
            _maxCopyTimeInMiliSec = maxCopyTimeInMiliSec;
        }

        public async Task<string?> ExtractZip()
        {
            if (!await UnlockFile())
            {
                return null;
            }

            string? rootDirectoryPath = FileHandler.ExtractZipAndGetRootDirPath(_zipPath, _rootDirectoryName);
            if (rootDirectoryPath == null)
            {
                return null;
            }

            string fileName = _rootDirectoryName;
            if (FileHandler.GetSingleFileInFolder(_rootDirectoryName, fileName) == null)
            {
                FileHandler.DeleteDirectoryContent(rootDirectoryPath, true);
                return null;
            }

            return rootDirectoryPath;
        }

        private async Task<bool> UnlockFile()
        {
            bool isFileLocked = true;
            int delayCounter = 2;
            while (delayCounter <= _maxCopyTimeInMiliSec && isFileLocked)
            {
                await Task.Delay(delayCounter);
                isFileLocked = FileHandler.IsFileLocked(_zipPath);
                delayCounter *= 2;
            }
            if (isFileLocked)
            {
                LogWriter.Log(LogLevel.Error, $"{_zipPath} nem volt elérhető {delayCounter} milisec után sem.");
                return false;
            }
            return true;
        }

    }
}
