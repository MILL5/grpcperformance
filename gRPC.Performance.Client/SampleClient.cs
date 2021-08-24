using Grpc.Net.Client;
using Performance.Contracts;
using ProtoBuf.Grpc.Client;
using System;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;

namespace gRPC.Performance.Client
{
    public class SampleClient : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly ISampleService _client;

        public SampleClient(string url = "https://localhost:5001")
        {
            CheckIsNotNullOrWhitespace(nameof(url), url);
            CheckIsWellFormedUri(nameof(url), url);

            _channel = GrpcChannel.ForAddress(url);
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

        public async ValueTask<Sample> GetSampleAsync(Identity id)
        {
            CheckIsNotNull(nameof(id), id);

            var result = await _client.GetSampleAsync(id);

            return result;
        }
    }
}
