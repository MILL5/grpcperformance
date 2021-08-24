using BenchmarkDotNet.Attributes;
using System;
using System.Threading.Tasks;

namespace gRPC.Benchmark
{
    public class BenchmarkTest
    {
        private static REST.Performance.Client.SampleClient grpcClient;

        static BenchmarkTest()
        {
            grpcClient = new REST.Performance.Client.SampleClient();
        }

        [Benchmark]
        public async Task RestTest()
        {
            var identity = new global::Performance.Models.Identity();
            var sample = await grpcClient.GetSampleAsync(identity);
            _ = sample.ID;
        }
    }
}
