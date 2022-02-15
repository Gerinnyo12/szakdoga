using System;
using System.IO;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class ZipHelper
    {
        // a .zip utvonala a monitorozott mappaban
        private readonly string _zipPath;
        private readonly string _rootDirectoryName;
        private readonly int _maxCopyTimeInMiliSec;

        public ZipHelper(string zipPath)
        {
            _zipPath = zipPath;
            _rootDirectoryName = FileHelper.GetFileName(zipPath, withoutExtension: true);
            _maxCopyTimeInMiliSec = App.MaxCopyTimeInMiliSec;
        }

        public async Task<string> ExtractZip()
        {
            if (!await UnlockFile())
            {
                return null;
            }

            string rootDirectoryPath = FileHelper.ExtractZipAndGetRootDirPath(_zipPath, _rootDirectoryName);
            if (rootDirectoryPath == null)
            {
                return null;
            }

            string fileName = _rootDirectoryName;
            if (FileHelper.GetSingleFileInFolder(_rootDirectoryName, fileName) == null)
            {
                FileHelper.DeleteDirectoryContent(rootDirectoryPath, true);
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
                isFileLocked = FileHelper.IsFileLocked(_zipPath);
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

    }
}
