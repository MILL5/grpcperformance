using BenchmarkDotNet.Attributes;
using Performance.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gRPC.Benchmark
{
    public class BenchmarkTest
    {
        private REST.Performance.Client.SampleClient restClient;
        private Identity[] ids;

        [Params(1, 10, 100, 1000)]
        public int BatchSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            restClient = new REST.Performance.Client.SampleClient();

            var ids = new List<Identity>();
            for (int i = 0; i < BatchSize; i++)
            {
                ids.Add(new Identity());
            }

            this.ids = ids.ToArray();
        }

        [Benchmark]
        public async Task RestTest()
        {
            var samples = await restClient.GetSamplesAsync(ids);
            _ = samples.Length;
        }
    }
}
