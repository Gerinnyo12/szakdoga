using Service.Helpers;
using Service.Interfaces;

namespace Service.Implementations
{
    public class ZipHandler : IZipHandler
    {
        private readonly ILogger<ZipHandler> _logger;

        public ZipHandler(ILogger<ZipHandler> logger)
        {
            _logger = logger;
        }

        public async Task<string?> ExtractZip(string zipPath, int maxCopyTimeInMiliSec)
        {
            if (string.IsNullOrEmpty(zipPath))
            {
                _logger.LogInformation($"A(z) {nameof(zipPath)} parameter nem lehet null vagy ures.");
                return null;
            }

            if (!await UnlockZip(zipPath, maxCopyTimeInMiliSec))
            {
                return null;
            }

            string? rootDirPath = ExtractZipAndGetRootDirPath(zipPath);
            if (rootDirPath is null)
            {
                return null;
            }

            return rootDirPath;
        }

        private async Task<bool> UnlockZip(string zipPath, int maxCopyTimeInMiliSec)
        {
            bool isUnlocked = false;
            int delayCounter = 2;
            while (delayCounter <= maxCopyTimeInMiliSec && !isUnlocked)
            {
                await Task.Delay(delayCounter);
                isUnlocked = IsZipUnlocked(zipPath);
                delayCounter *= 2;
            }
            if (!isUnlocked)
            {
                _logger.LogError($"A(z) {zipPath} masolasa nem lett kesz idoben.");
            }
            return isUnlocked;
        }

        private string? ExtractZipAndGetRootDirPath(string zipPath)
        {
            string rootDirName = FileHelper.GetFileName(zipPath, withoutExtension: true);
            string destinationDirPath = FileHelper.CombinePaths(FileHelper.LocalDir, rootDirName);
            try
            {
                // EZ MERGELI NEM PEDIG FELULIRJA
                FileHelper.ExtractZip(zipPath, destinationDirPath);
                return destinationDirPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Nem sikerult a(z) {zipPath} kicsomagolasa a(z) {rootDirName} mappaba.");
            }
            return null;
        }

        private bool IsZipUnlocked(string zipPath)
        {
            try
            {
                using FileStream stream = FileHelper.OpenFile(zipPath);
                return true;
            }
            catch (Exception ex)
            {
                //TODO HA NEM AZZAL VAN A BAJ, HOGY NEM ELERHETO A FILE
                _logger.LogInformation(ex, $"A(z) {zipPath} masolasa meg mindig folyamatban van...");
            }
            return false;
        }

    }
}
