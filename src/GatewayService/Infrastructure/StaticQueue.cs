using System.Collections.Concurrent;

namespace GatewayService.Infrastructure;

public static class StaticQueue
{
    public static BlockingCollection<SerializableHttpRequest> Queue = 
        new BlockingCollection<SerializableHttpRequest>();
    private static object _lock = new();
    public class SerializableHttpRequest
    {
        public string Method { get; set; }
        public Uri RequestUri { get; set; }
        public Dictionary<string, string[]> Headers { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    
        public async Task<HttpRequestMessage> ToHttpRequestMessage()
        {
            var request = new HttpRequestMessage(
                new HttpMethod(Method), 
                RequestUri
            );
            
            foreach (var header in Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            
            if (Content != null)
            {
                request.Content = new ByteArrayContent(Content);
                if (!string.IsNullOrEmpty(ContentType))
                {
                    request.Content.Headers.ContentType = 
                        System.Net.Http.Headers.MediaTypeHeaderValue.Parse(ContentType);
                }
            }
        
            return request;
        }
    }

    public static async Task<SerializableHttpRequest> FromHttpRequestMessage(
        this HttpRequestMessage request)
    {
        var serializable = new SerializableHttpRequest
        {
            Method = request.Method.Method,
            RequestUri = request.RequestUri,
            Headers = request.Headers
                .ToDictionary(h => h.Key, h => h.Value.ToArray())
        };
    
        if (request.Content != null)
        {
            serializable.Content = await request.Content.ReadAsByteArrayAsync();
            serializable.ContentType = request.Content.Headers.ContentType?.ToString();
        }
    
        return serializable;
    }

    public static async Task AddMessage(HttpRequestMessage message)
    {
        Console.WriteLine($"Adding message {message.Method} {message.RequestUri}");
        Queue.Add(await message.FromHttpRequestMessage());
    }

}