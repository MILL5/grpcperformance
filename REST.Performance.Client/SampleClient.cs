using Performance.Contracts;
using Performance.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;

namespace REST.Performance.Client
{
    public class SampleClient : ISampleService, IDisposable
    {
        private readonly string _url;
        private readonly HttpClient _client;

        public SampleClient(string url = "https://localhost:5001")
        {
            CheckIsNotNullOrWhitespace(nameof(url), url);
            CheckIsWellFormedUri(nameof(url), url);

            _url = url;
            _client = new HttpClient();
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

        public async ValueTask<Sample> GetSampleAsync(Identity id)
        {
            CheckIsNotNull(nameof(id), id);

            var requestUri = $"{_url}/api/Sample/{id.Catalog}/{id.ID}";

            return await _client.GetFromJsonAsync<Sample>(requestUri);
        }
    }
}
