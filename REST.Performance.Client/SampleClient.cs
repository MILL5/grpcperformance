using M5.BloomFilter;
using Performance;
using Pineapple.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;
using static Pineapple.Common.Cleanup;

namespace REST.Performance.Client
{
    public class SampleClient : IDisposable
    {
        private readonly string _url;
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions _jsonSerializerOptions;
        private static readonly Parallelism _parallelism;
        private static readonly object _instanceLock = new();
        private static int _instanceCount = 0;
        private volatile VersionedResponse<ReadOnlyFilter> _bloomFilter;
        private volatile bool _isrunning;
        
        static SampleClient()
        {
            _parallelism = new Parallelism();
            ServicePointManager.DefaultConnectionLimit = Math.Max(ServicePointManager.DefaultConnectionLimit, _parallelism.MaxDegreeOfParallelism);
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.Converters.Add(new BitArrayConverter());
            jsonSerializerOptions.Converters.Add(new BloomFilterConverter());
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public SampleClient(string url = "https://localhost:5001", Version httpVersion = null)
        {
            CheckIsNotNullOrWhitespace(nameof(url), url);
            CheckIsWellFormedUri(nameof(url), url);

            EnsureDefaultConnections();

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
            _isrunning = true;
            Task.Run(UpdateBloomFilter).ConfigureAwait(false);
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
            _isrunning = false;
            SafeMethod(_client.Dispose);

            SafeMethod(() =>
            {
                lock (_instanceLock)
                {
                    _instanceCount--;
                }
            });
        }

        private void EnsureDefaultConnections()
        {
            //
            // We grow the default connection limit to (# of Outstanding Clients) * _parallelism.MaxDegreeOfParallelism
            //

            int limit;

            lock (_instanceLock)
            {
                _instanceCount++;
                limit = Math.Max(ServicePointManager.DefaultConnectionLimit, _parallelism.MaxDegreeOfParallelism * _instanceCount);
            }

            if (limit > ServicePointManager.DefaultConnectionLimit)
            {
                ServicePointManager.DefaultConnectionLimit = limit;
            }
        }

        private async Task UpdateBloomFilter()
        {
            const int fiveminutes = 5 * 60 * 1000;

            while (_isrunning)
            {
                try
                {
                    var version = _bloomFilter.Version;

                    var bloomFilter = GetBloomFilterAsync(version).Result;

                    if ((bloomFilter.Update != VersionUpdate.None) &&
                        (bloomFilter.Value != null))
                    {
                        _bloomFilter = bloomFilter;
                    }
                }
                catch
                {
                }

                await Task.Delay(fiveminutes);
            }
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

        public async ValueTask<Sample[]> GetSamplesFromCacheAsync(Identity[] ids, bool useBloomFilter = true, bool useMultiplexing = true)
        {
            CheckIsNotNull(nameof(ids), ids);

            Sample[] result;

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

            if (useMultiplexing)
            {
                int multiPlexingSize = _parallelism.MaxDegreeOfParallelism;

                var splitThis = new List<Identity>(ids.Length);
                splitThis.AddRange(idsToSend);
                var batches = splitThis.Split(multiPlexingSize);

                var results = new List<Sample>(ids.Length);

                var parallelResult = Parallel.ForEach(batches, _parallelism.Options, (p) =>
                {
                    var idsToSend = p.ToArray();

                    var response = _client.PostAsJsonAsync(requestUri, idsToSend).Result;
                    response.EnsureSuccessStatusCode();
                    var s = response.Content.ReadFromJsonAsync<Sample[]>().Result;

                    lock (results)
                    {
                        results.AddRange(s);
                    }
                });

                if (!parallelResult.IsCompleted)
                    throw new Exception("The entire batch was not successful.");

                result = results.ToArray();
            }
            else
            {
                var response = await _client.PostAsJsonAsync(requestUri, idsToSend);
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadFromJsonAsync<Sample[]>();
            }

            return result;
        }
    }
}
