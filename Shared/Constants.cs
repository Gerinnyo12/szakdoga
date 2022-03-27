using Shared.Helpers;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;

namespace Shared
{
    public static class Constants
    {
        public static readonly string PARAMETERS_NAME = "Parameters";
        public static readonly string SHARED_PROJECT_NAME = "Shared";
        public static readonly string I_WORKER_TASK = "IWorkerTask";
        public static readonly string DLL_EXTENSION = ".dll";
        public static readonly string ZIP_EXTENSION = ".zip";

        public static readonly IPAddress IP_ADDRESS = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
        public static readonly int PORT = 5678;
        public static readonly string RESPONSE_STRING_WHEN_LISTENER_STOPS = "Leall a listener!";
        public static readonly string RESPONSE_JSON_WHEN_LISTENER_STOPS = JsonHelper.Serialize(RESPONSE_STRING_WHEN_LISTENER_STOPS);
        public static readonly double MAX_SECONDS_TO_WAIT_FOR_SERVICE = 10;
        public static readonly int MAX_LENGTH_OF_TCP_RESPONSE = 10000;

        public static readonly string SERVICE_NAME = "Scheduler";
        public static readonly string SERVICE_DIR_PATH;

        public static readonly string APP_SETTINGS_JSON = "appsettings.json";
        public static readonly string APP_SETTINGS_JSON_PATH;

        public static readonly string MONITORING_DIR_NAME = "Monitor";
        public static readonly string MONITORING_DIR_PATH;

        public static readonly string LOCAL_DIR_NAME = "Local";
        public static readonly string LOCAL_DIR_PATH;

        public static readonly string RUNNER_DIR_NAME = "Run";
        public static readonly string RUNNER_DIR_PATH;

        public static readonly bool IS_WINDOWS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        static Constants()
        {
            if (!IS_WINDOWS) return;

            //ha valami hibat dob akkor ne is induljon el a szerviz
            //ezert nem itt kapom el
            using (ManagementObject wmiService = new($"Win32_Service.Name='{SERVICE_NAME}'"))
            {
                wmiService?.Get();
                string serviceExePath = wmiService["PathName"]?.ToString();
                SERVICE_DIR_PATH = FileHelper.GetDirParent(serviceExePath);
                APP_SETTINGS_JSON_PATH = FileHelper.CombinePaths(SERVICE_DIR_PATH, APP_SETTINGS_JSON);
                MONITORING_DIR_PATH = FileHelper.CombinePaths(SERVICE_DIR_PATH, MONITORING_DIR_NAME);
                LOCAL_DIR_PATH = FileHelper.CombinePaths(SERVICE_DIR_PATH, LOCAL_DIR_NAME);
                RUNNER_DIR_PATH = FileHelper.CombinePaths(SERVICE_DIR_PATH, RUNNER_DIR_NAME);
            }
            PrepareDir(MONITORING_DIR_PATH);
            PrepareDir(LOCAL_DIR_PATH, clearDir: true);
            PrepareDir(RUNNER_DIR_PATH, clearDir: true);
        }

        private static void PrepareDir(string dirPath, bool clearDir = false)
        {
            if (clearDir && FileHelper.DirExists(dirPath))
            {
                //akkor dob hibat, ha az ui inditasakor mar van futo projekt
                try { FileHelper.DeleteDir(dirPath); } catch { }
            }
            FileHelper.CreateDir(dirPath);
        }
    }
}
