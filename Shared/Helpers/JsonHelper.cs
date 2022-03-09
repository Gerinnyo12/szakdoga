using System.Text.Json;


namespace Shared.Helpers
{
    public class JsonHelper
    {
        public static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public static async Task<string> SerializeAsync(object obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, obj);
            stream.Position = 0;
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static Tout Deserialize<Tout>(string json)
        {
            if (json is null)
            {
                throw new ArgumentNullException(nameof(json));
            }
            Tout? obj = JsonSerializer.Deserialize<Tout>(json);
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            return obj;
        }
    }
}
