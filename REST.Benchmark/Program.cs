using BenchmarkDotNet.Running;
using System;

namespace Benchmark
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Hit any key to start.");
            Console.ReadLine();

            var summarys = BenchmarkRunner.Run(typeof(Program).Assembly, new BenchmarkConfig());

            foreach (var s in summarys)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine();
            Console.WriteLine("Hit any key to exit.");
            Console.ReadLine();
        }
    }
}
