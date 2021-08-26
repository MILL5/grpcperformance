using REST.Performance.Client;
using System;
using System.Threading.Tasks;
using Performance;
using System.Collections.Generic;

namespace REST
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
