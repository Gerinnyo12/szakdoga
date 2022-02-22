using Service.Helpers;

namespace Service.Components
{
    public class Arguments
    {
        private const int ARGS_NUM = 3;

        public static (string, string, int) ParseArguments(string[] args)
        {
            if (args == null || args.Length != ARGS_NUM)
            {
                throw new ArgumentNullException($"Az argumentumok száma nem {ARGS_NUM}");
            }
            if (!FileHelper.DirExists(args[0]))
            {
                throw new ArgumentException($"Nem létezik a(z) {args[1]} mappa");
            }
            if (!int.TryParse(args[2], out int copyTime))
            {
                throw new ArgumentException($"Nem lehet számmá konvertálni a 3. paramétert");
            }
            return (args[0], args[1] + ".zip", copyTime);
        }
    }
}
