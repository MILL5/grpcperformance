using REST.Performance.Client;
using Performance.Contracts;
using System;
using System.Threading.Tasks;
using Performance.Models;

namespace REST
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hit any key to start.");
            Console.ReadLine();

            var client = new SampleClient();

            for (int i = 0; i < 100; i++)
            {
                var id = new Identity();
                var sample = await client.GetSampleAsync(id);
                Console.Write($"Type:{sample.SampleType}");

                foreach (var v in sample.Values)
                {
                    Console.Write($",Value:{v.Value},Threshold:{v.Threshold}");
                }

                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}
