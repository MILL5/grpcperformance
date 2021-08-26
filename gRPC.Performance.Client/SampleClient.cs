using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Compression;
using Performance;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;

namespace gRPC.Performance.Client
{
    public class SampleClient : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly ISampleService _client;
        private readonly CallOptions _callOptions;

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

        public async ValueTask<Sample[]> GetSamplesAsync(Identity[] ids)
        {
            CheckIsNotNull(nameof(ids), ids);

            var result = await _client.GetSampleAsync(ids, _callOptions);

            return result;
        }
    }
}
