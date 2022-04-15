using Service.Interfaces;
using Shared.Helpers;

namespace Service.Implementations
{
    public class ZipExtracter : IZipExtracter
    {
        private readonly ILogger<ZipExtracter> _logger;

        public ZipExtracter(ILogger<ZipExtracter> logger) => _logger = logger;

        public async Task<string?> ExtractZip(string zipPath, int maxCopyTimeInMiliSec)
        {
            if (string.IsNullOrEmpty(zipPath))
            {
                _logger.LogInformation("A(z) {nameof(zipPath)} paraméter se null se üres nem lehet.", nameof(zipPath));
                return null;
            }

            if (!await UnlockZip(zipPath, maxCopyTimeInMiliSec)) return null;

            string? rootDirPath = ExtractZipAndGetRootDirPath(zipPath);
            if (rootDirPath is null) return null;

            return rootDirPath;
        }

        private async Task<bool> UnlockZip(string zipPath, int maxCopyTimeInMiliSec)
        {
            bool isUnlocked = false;
            int delayCounter = 2;
            while (delayCounter <= maxCopyTimeInMiliSec && !isUnlocked)
            {
                //itt valt at async-re a futas
                await Task.Delay(delayCounter);
                isUnlocked = IsZipUnlocked(zipPath);
                delayCounter *= 2;
            }
            if (!isUnlocked)
            {
                _logger.LogError("A(z) {zipPath} másolasá nem lett kész időben.", zipPath);
            }

            return isUnlocked;
        }

        private string? ExtractZipAndGetRootDirPath(string zipPath)
        {
            string rootDirName = FileHelper.GetFileName(zipPath, withoutExtension: true);
            string destinationDirPath = FileHelper.GetAbsolutePathOfLocalDir(rootDirName);

            try
            {
                //EZ MERGELI NEM PEDIG FELULIRJA
                FileHelper.ExtractZip(zipPath, destinationDirPath);
                return destinationDirPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nem sikerült a(z) {zipPath} kicsomagolása a(z) {rootDirName} mappába.", zipPath, rootDirName);
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
                _logger.LogInformation(ex, "A(z) {zipPath} másolása még mindig folyamatban van...", zipPath);
            }

            return false;
        }

    }
}
