using System.Net.Sockets;
using System.Text;

namespace Shared.Helpers
{
    public class ListenerHelper
    {
        private static readonly int _maxMessageLength = Constants.MAX_LENGTH_OF_TCP_RESPONSE;
        public static async Task<string> GetStringFromStream(NetworkStream stream)
        {
            var request = new byte[_maxMessageLength];
            int bytesRead = await stream.ReadAsync(request, 0, request.Length);
            return GetStringFromBytes(request, bytesRead);
        }

        private static string GetStringFromBytes(byte[] bytes, int lengthOfBytes) => 
            Encoding.UTF8.GetString(bytes, 0, lengthOfBytes);

        public static async Task WriteJsonToStream(NetworkStream stream, string json)
        {
            var response = MakeBytesFromString(json);
            await stream.WriteAsync(response, 0, response.Length);
        }

        private static byte[] MakeBytesFromString(string json) => 
            Encoding.UTF8.GetBytes(json);

    }
}
