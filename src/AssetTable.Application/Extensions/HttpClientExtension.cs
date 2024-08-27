using System.Net.Http;
using System.Threading.Tasks;

namespace AssetTable.Application.Extension
{
    public static class HttpClientExtension
    {
        public static Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
            {
                Content = content
            };
            return client.SendAsync(request);
        }
    }
}