using BenchmarkDotNet.Attributes;
using System.Linq;
using gRPC.Performance;
using Performance.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gRPC.Benchmark
{
    public class BenchmarkTest
    {
        private Performance.Client.SampleClient grpcClient;
        private Identity[] ids;
        private IEnumerable<Identity[]> parts;

        [Params(1, 10, 100, 1000)]
        public int BatchSize { get; set; }

        public int NumberOfParts
        {
            get
            {
                return BatchSize >= 5 ? 5 : 1;
            }
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            grpcClient = new Performance.Client.SampleClient();

            var ids = new List<Identity>();
            for (int i = 0; i < BatchSize; i++)
            {
                ids.Add(new Identity());
            }

            this.ids = ids.ToArray();

            var parts = ids.Split(NumberOfParts);
            var p = new List<Identity[]>(NumberOfParts);

            foreach (var part in parts)
            {
                p.Add(part.ToArray());
            }

            this.parts = p;
        }

        [Benchmark]
        public async Task GrpcTest()
        {
            var samples = await grpcClient.GetSamplesAsync(ids);
            _ = samples.Length;
        }

        [Benchmark]
        public async Task GrpcParallelTest()
        {
            var samples = new List<Sample>();

            Parallel.ForEach(parts, (p) =>
            {
                var s = grpcClient.GetSamplesAsync(p).Result;

                lock (samples)
                {
                    samples.AddRange(s);
                }
            });

            _ = samples.ToArray().Length;

            await ValueTask.CompletedTask;
        }
    }
}
