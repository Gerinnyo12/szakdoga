using Service.Helpers;
using Service.Interfaces;

namespace Service.Components
{
    public class ZipHandler : IZipHandler
    {
        // a .zip utvonala a monitorozott mappaban
        private readonly string _zipPath;
        private readonly string _rootDirName;
        private readonly int _maxCopyTimeInMiliSec;

        public ZipHandler(string zipPath, int maxCopyTimeInMiliSec)
        {
            _zipPath = zipPath;
            _rootDirName = FileHelper.GetFileName(zipPath, withoutExtension: true);
            _maxCopyTimeInMiliSec = maxCopyTimeInMiliSec;
        }

        public async Task<string?> ExtractZip()
        {
            if (!await UnlockZip())
            {
                return null;
            }

            string? rootDirPath = ExtractZipAndGetRootDirPath();
            if (rootDirPath == null)
            {
                return null;
            }

            return rootDirPath;
        }

        private async Task<bool> UnlockZip()
        {
            bool isFileLocked = true;
            int delayCounter = 2;
            while (delayCounter <= _maxCopyTimeInMiliSec && isFileLocked)
            {
                await Task.Delay(delayCounter);
                isFileLocked = IsZipLocked();
                delayCounter *= 2;
            }
            if (isFileLocked)
            {
                LogWriter.Log(LogLevel.Error, $"{_zipPath} nem volt elérhető {delayCounter} milisec után sem.");
                return false;
            }
            return true;
        }

        private string? ExtractZipAndGetRootDirPath()
        {
            string destinationDirPath = FileHelper.CombinePaths(FileHelper.LocalDir, _rootDirName);
            try
            {
                // EZ MERGELI NEM PEDIG FELULIRJA
                FileHelper.ExtractZip(_zipPath, destinationDirPath);
                return destinationDirPath;
            }
            catch (Exception ex)
            {
                LogWriter.Log(LogLevel.Error, $"Nem sikerult a(z) {_rootDirName} nevu zip kicsomagolasa: {ex.Message}");
            }
            return null;
        }

        private bool IsZipLocked()
        {
            try
            {
                using FileStream stream = FileHelper.OpenFile(_zipPath);
                return false;
            }
            catch (Exception ex)
            {
                //TODO LOGOLNI HA NEM AZZAL VAN A BAJ, HOGY NEM ELERHETO A FILE
                LogWriter.Log(LogLevel.Error, $"{ex.Message}");
            }
            return true;
        }

    }
}
