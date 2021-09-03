using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Compression;
using M5.BloomFilter;
using Performance;
using Pineapple.Threading;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;

namespace gRPC.Performance.Client
{
    public class SampleClient : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly ISampleService _client;
        private readonly CallOptions _callOptions;
        private VersionedResponse<ReadOnlyFilter> _bloomFilter;

        public SampleClient(string url = "https://localhost:5001")
        {
            CheckIsNotNullOrWhitespace(nameof(url), url);
            CheckIsWellFormedUri(nameof(url), url);

            var httpClientHandler = new SocketsHttpHandler
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                EnableMultipleHttp2Connections = true
            };

            var options = new GrpcChannelOptions { HttpHandler = httpClientHandler };
            options.CompressionProviders = new List<ICompressionProvider> { new GzipCompressionProvider(CompressionLevel.Optimal) };

            var headers = new Metadata();
            headers.Add("grpc-accept-encoding", "gzip");

            _callOptions = new CallOptions(headers: headers);
            _channel = GrpcChannel.ForAddress(url, options);
            _client = _channel.CreateGrpcService<ISampleService>();

            _bloomFilter = GetBloomFilterAsync().Result;
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
            _channel.Dispose();
        }

        private async ValueTask<VersionedResponse<ReadOnlyFilter>> GetBloomFilterAsync(global::Performance.VersionInfo versionInfo = null)
        {
            var response = await _client.GetBloomFilterAsync(versionInfo, _callOptions);

            var bloomFilter = response.Value.AsBloomFilter();

            var result = new VersionedResponse<ReadOnlyFilter> { Update = response.Update, Version = response.Version, Value = bloomFilter };

            return result;
        }

        public async ValueTask<Sample[]> GetSamplesAsync(Identity[] ids)
        {
            CheckIsNotNull(nameof(ids), ids);

            var result = await _client.GetSampleAsync(ids, _callOptions);

            return result;
        }

        public async ValueTask<Sample[]> GetSamplesFromCacheAsync(Identity[] ids,
                                        bool useBloomFilter = true,
                                        bool useMultiplexing = true)
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
            Sample[] result;

            if (useMultiplexing)
            {
                var parallelism = new Parallelism();
                int multiPlexingSize = parallelism.MaxDegreeOfParallelism;

                var splitThis = new List<Identity>(ids.Length);
                splitThis.AddRange(idsToSend);
                var batches = splitThis.Split(multiPlexingSize);

                var results = new List<Sample>(ids.Length);

                Parallel.ForEach(batches, parallelism.Options, (p) =>
                {
                    var idsToSend = p.ToArray();
                    var s = _client.GetSampleFromCacheAsync(idsToSend, _callOptions).Result;

                    lock (results)
                    {
                        results.AddRange(s);
                    }
                });

                result = results.ToArray();
            }
            else
            {
                result = await _client.GetSampleFromCacheAsync(idsToSend, _callOptions);
            }

            return result;
        }
    }
}
