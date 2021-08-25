using gRPC.Performance.Client;
using Performance.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gRPC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hit any key to start.");
            Console.ReadLine();

            var client = new SampleClient();

            var ids = new List<Identity>();

            for (int i = 0; i < 100; i++)
            {
                var id = new Identity();
                ids.Add(id);
            }

            var samples = await client.GetSamplesAsync(ids.ToArray());
            foreach (var s in samples)
            {
                Console.Write($"Type:{s.SampleType}");

                foreach (var v in s.Values)
                {
                    Console.Write($",Value:{v.Value},Threshold:{v.Threshold}");
                }
            }

            Console.WriteLine();

            Console.ReadLine();
        }
    }
}
