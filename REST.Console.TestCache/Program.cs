using Performance;
using REST.Performance.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Pineapple.Common.Preconditions;

namespace REST
{
    class Program
    {
        private const int CatalogSize = 70000000;
        private const int BatchSize = 1;
        private const int NumOfBatches = CatalogSize / BatchSize;
        private const bool UseBloomFilter = true;
        private const bool UseMultiplexing = true;

        static async Task Main()
        {
            Console.WriteLine("Hit any key to start.");
            Console.ReadLine();

            Console.WriteLine("Creating test data...");

            var client = new SampleClient();

            var ids = new List<Identity>();

            for (int i = 0; i < CatalogSize; i++)
            {
                var id = new Identity { Catalog = 1, ID = i };
                ids.Add(id);
            }

            Console.WriteLine("Created test data!");
            Console.WriteLine("Creating batches...");

            int found = 0;
            var batches = ids.Split(NumOfBatches);
            var firstBatchCount = batches.First().Count();
            var lastBatchCount = batches.Last().Count();

            CheckIsEqualTo(nameof(firstBatchCount), firstBatchCount, BatchSize);
            CheckIsEqualTo(nameof(lastBatchCount), lastBatchCount, BatchSize);

            Console.WriteLine("Created batches!");

            GC.Collect();

            Console.WriteLine("Sending batches ...");

            int batchCount = 0;
            var sw = new Stopwatch();
            sw.Start();

            foreach (var batch in batches)
            {
                var samples = await client.GetSamplesFromCacheAsync(batch.ToArray(), useBloomFilter: UseBloomFilter, useMultiplexing: UseMultiplexing);
                found += samples.Length;

                batchCount++;

                if (batchCount % 1000 == 0)
                {
                    Console.WriteLine($"{batchCount} batches sent.");
                }
            }

            var elapsedTime = sw.Elapsed;

            sw.Stop();

            Console.WriteLine("Batches complete!");
            
            CheckIsEqualTo(nameof(found), found, CatalogSize / 10);

            Console.WriteLine($"REST Cache Test:  Batch Size={BatchSize},Bloom Filter={UseBloomFilter},Multiplexing={UseMultiplexing}");
            Console.WriteLine("===============================================================================");
            Console.WriteLine($"Found {found} out of {CatalogSize} items in {elapsedTime.TotalMinutes} minutes");
            Console.WriteLine();

            Console.ReadLine();
        }
    }
}
