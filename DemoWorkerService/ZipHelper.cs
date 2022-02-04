using System;
using System.IO;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    public class ZipHelper
    {
        // a .zip utvonala a monitorozott mappaban
        public readonly string ZipPath;

        private readonly string _rootDirectoryName;

        private readonly string _startingFileName;

        private readonly int _maxCopyTimeInMiliSec;

        public ZipHelper(string zipPath)
        {
            ZipPath = zipPath;
            _rootDirectoryName = Path.GetFileNameWithoutExtension(zipPath);
            _startingFileName = _rootDirectoryName;
            _maxCopyTimeInMiliSec = App.MaxCopyTimeInMiliSec;
        }

        public async Task<string> ExtractZip()
        {
            if (!await UnlockFile())
            {
                return null;
            }

            string rootDirectoryPath = FileHelper.ExtractZipAndGetRootDirPath(ZipPath, _rootDirectoryName);
            if (rootDirectoryPath == null)
            {
                return null;
            }

            if (FileHelper.GetSingleFileInFolder(_rootDirectoryName, _startingFileName) == null)
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
                isFileLocked = FileHelper.IsFileLocked(ZipPath);
                delayCounter *= 2;
            }
            if (isFileLocked)
            {
                //TODO LOG 
                Console.WriteLine($"{ZipPath} nem volt elérhető {delayCounter} milisec után sem.");
                return false;
            }
            return true;
        }

    }
}
