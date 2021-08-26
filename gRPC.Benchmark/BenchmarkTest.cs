using BenchmarkDotNet.Attributes;
using System.Linq;
using Performance;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace gRPC.Benchmark
{
    public class BenchmarkTest
    {
        private Performance.Client.SampleClient grpcClient;
        private Identity[] ids;
        private IEnumerable<Identity[]> parts;
        private int _batchSize;

        [Params(1, 10, 50, 100, 500, 1000, 10000)]
        public int BatchSize
        {
            get { return _batchSize; }
            set
            {
                if (value <= 1)
                    NumberOfParts = 1;
                else if (value <= 10)
                    NumberOfParts = 2;
                else if (value <= 50)
                    NumberOfParts = 2;
                else if (value <= 100)
                    NumberOfParts = 3;
                else if (value <= 500)
                    NumberOfParts = 4;
                else
                    NumberOfParts = 5;

                _batchSize = value;
            }
        }

        public int NumberOfParts { get; set; }

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
