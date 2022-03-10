using Shared.Helpers;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;

namespace Shared
{
    public static class Constants
    {
        public static string PARAMETERS_NAME = "Parameters";
        public static string SHARED_PROJECT_NAME = "Shared";
        public static string I_WORKER_TASK = "IWorkerTask";
        public static string DLL_EXTENSION = ".dll";
        public static string RESPONSE_STRING_WHEN_LISTENER_STOPS = "Leall a listener!";
        public static string RESPONSE_JSON_WHEN_LISTENER_STOPS = JsonHelper.Serialize(RESPONSE_STRING_WHEN_LISTENER_STOPS);
        public static double MAX_SECONDS_TO_WAIT_FOR_SERVICE = 15;
        public static IPAddress IP_ADDRESS = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
        public static int PORT = 5678;
        public static int MAX_LENGTH_OF_TCP_RESPONSE = 10000;

        public static string SERVICE_NAME = "Scheduler";
        public static string SERVICE_DIR_PATH;

        private static string APP_SETTINGS_JSON = "appsettings.json";
        public static string APP_SETTINGS_JSON_PATH;

        private static string LOCAL_DIR_NAME = "Local";
        public static string LOCAL_DIR_PATH;

        private static string RUNNER_DIR_NAME = "Runner";
        public static string RUNNER_DIR_PATH;

        public static bool IS_WINDOWS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        static Constants()
        {
            if (!IS_WINDOWS) return;

            using (ManagementObject wmiService = new($"Win32_Service.Name='{SERVICE_NAME}'"))
            {
                wmiService?.Get();
                string serviceExePath = wmiService["PathName"].ToString()!;
                SERVICE_DIR_PATH = Directory.GetParent(serviceExePath)!.ToString();
                APP_SETTINGS_JSON_PATH = Path.Combine(SERVICE_DIR_PATH, APP_SETTINGS_JSON);
                LOCAL_DIR_PATH = Path.Combine(SERVICE_DIR_PATH, LOCAL_DIR_NAME);
                RUNNER_DIR_PATH = Path.Combine(SERVICE_DIR_PATH, RUNNER_DIR_NAME);
            }
            PrepareDir(LOCAL_DIR_NAME);
            PrepareDir(RUNNER_DIR_PATH);
        }

        private static void PrepareDir(string dirPath)
        {
            try { Directory.Delete(dirPath, true); } catch { }
            Directory.CreateDirectory(dirPath);
        }
    }
}
