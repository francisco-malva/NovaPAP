using System.Net;
using System.Text;
using System.Text.Json;

namespace ScoringServer.Extensions;

public static class HttpResponseExtensions
{
    public static async Task WriteJson<T>(this HttpListenerResponse response, T data)
    {
        response.ContentEncoding = Encoding.UTF8;
        response.ContentType = "application/json";
        await JsonSerializer.SerializeAsync(response.OutputStream, data);
    }
}