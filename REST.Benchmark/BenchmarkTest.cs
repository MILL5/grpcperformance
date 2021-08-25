using BenchmarkDotNet.Attributes;
using gRPC.Performance;
using Performance.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gRPC.Benchmark
{
    public class BenchmarkTest
    {
        private static readonly REST.Performance.Client.SampleClient restClient;
        private static readonly Identity[] ids;

        static BenchmarkTest()
        {
            restClient = new REST.Performance.Client.SampleClient();

            var ids = new List<Identity>();
            for (int i = 0; i < Constants.BatchSize; i++)
            {
                ids.Add(new Identity());
            }

            BenchmarkTest.ids = ids.ToArray();
        }

        [Benchmark]
        public async Task RestTest()
        {
            var samples = await restClient.GetSamplesAsync(ids);
            _ = samples.Length;
        }
    }
}
