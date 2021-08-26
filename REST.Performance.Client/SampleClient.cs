using Performance;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;

namespace REST.Performance.Client
{
    public class SampleClient : ISampleService, IDisposable
    {
        private readonly string _url;
        private readonly HttpClient _client;

        public SampleClient(string url = "https://localhost:5001", Version version = null)
        {
            CheckIsNotNullOrWhitespace(nameof(url), url);
            CheckIsWellFormedUri(nameof(url), url);

            var handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            };

            var hc = new HttpClient(handler);

            if (version != null)
            {
                hc.DefaultRequestVersion = version;
            }

            _url = url;
            _client = hc;
        }

        ~SampleClient()
        {
            DisposeInternal();
        }

        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeInternal()
        {
            _client.Dispose();
        }

        public async ValueTask<Sample[]> GetSamplesAsync(Identity[] ids)
        {
            CheckIsNotNull(nameof(ids), ids);

            var requestUri = $"{_url}/api/v1/Sample";

            var response = await _client.PostAsJsonAsync(requestUri, ids);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Sample[]>();
        }

        public ValueTask<Sample[]> GetSamplesFromCacheAsync(Identity[] ids)
        {
            throw new NotImplementedException();
        }
    }
}
