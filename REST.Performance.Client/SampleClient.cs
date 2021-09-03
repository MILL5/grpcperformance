using M5.BloomFilter;
using Performance;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;

namespace REST.Performance.Client
{
    public class SampleClient : IDisposable
    {
        private readonly string _url;
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions _jsonSerializerOptions;
        private VersionedResponse<ReadOnlyFilter> _bloomFilter;

        static SampleClient()
        {
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new BitArrayConverter());
            jsonSerializerOptions.Converters.Add(new BloomFilterConverter());
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public SampleClient(string url = "https://localhost:5001", Version httpVersion = null)
        {
            CheckIsNotNullOrWhitespace(nameof(url), url);
            CheckIsWellFormedUri(nameof(url), url);

            var handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            };

            var hc = new HttpClient(handler);

            if (httpVersion != null)
            {
                hc.DefaultRequestVersion = httpVersion;
            }

            _url = url;
            _client = hc;
            var bloomFilter = GetBloomFilterAsync().Result;
            _bloomFilter = bloomFilter;
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

        private async Task<VersionedResponse<ReadOnlyFilter>> GetBloomFilterAsync(VersionInfo versionInfo = null)
        {
            var requestUri = $"{_url}/api/v1/Sample/bloomfilter";

            if (versionInfo != null)
            {
                requestUri = $"{requestUri}?serverId={versionInfo.ServerId}&version={versionInfo.Version}";
            }

            var response = await _client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<VersionedResponse<ReadOnlyFilter>>(_jsonSerializerOptions);
            return result;
        }

        public async ValueTask<Sample[]> GetSamplesAsync(Identity[] ids, bool useBloomFilter = false)
        {
            CheckIsNotNull(nameof(ids), ids);

            var requestUri = $"{_url}/api/v1/Sample";

            var response = await _client.PostAsJsonAsync(requestUri, ids);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Sample[]>();
        }

        public async ValueTask<Sample[]> GetSamplesFromCacheAsync(Identity[] ids, bool useBloomFilter = false)
        {
            CheckIsNotNull(nameof(ids), ids);

            List<Identity> filteredIds = null;
            var bf = _bloomFilter?.Value;

            if (useBloomFilter && bf != null)
            {
                
                filteredIds = new List<Identity>();

                foreach (var id in ids)
                {
                    if (bf.Contains(id))
                    {
                        filteredIds.Add(id);
                    }
                }
            }

            var idsToSend = filteredIds == null ? ids : filteredIds.ToArray();

            var requestUri = $"{_url}/api/v1/Sample/fromcache";

            var response = await _client.PostAsJsonAsync(requestUri, idsToSend);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Sample[]>();
        }
    }
}
