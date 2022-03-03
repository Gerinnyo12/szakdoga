using System.Text.Json;

namespace UI.Helpers
{
    public class JsonConverter
    {
        public static string Serialize(object? obj)
        {
            return JsonSerializer.Serialize(obj);
        }
    }
}
