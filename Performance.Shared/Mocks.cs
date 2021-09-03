using System;
using System.Collections.Generic;
using System.Text;
using Performance;

namespace Performance.Shared
{
    public static class Mocks
    {
        // Consistent seed
        private static readonly Random _random = new(97);

        private static int GetRandomInt(int maxValue = int.MaxValue)
        {
            return _random.Next(0, maxValue);
        }

        private static SampleData GetSampleData()
        {
            int random = GetRandomInt(100);

            decimal value = random * 1000m / 100m;
            decimal threshold = random * 5m / 100m;

            if (random < 10)
                return new SampleData { DataType = DataType.Dollar, Threshold = threshold, Value = value };

            return new SampleData { DataType = DataType.Percentage, Threshold = threshold, Value = value };
        }

        public static Sample GetSample()
        {
            int random = GetRandomInt(100);

            if (random > 50)
            {
                return new Sample
                {
                    ID = new Identity { Catalog = GetRandomInt(), ID = GetRandomInt() },
                    SampleType = GetRandomInt(5),
                    Values = new[] { GetSampleData() }
                };
            }
            else
            {
                return new Sample
                {
                    ID = new Identity { Catalog = GetRandomInt(), ID = GetRandomInt() },
                    SampleType = GetRandomInt(5),
                    Values = new[] { GetSampleData(), GetSampleData() }
                };
            }
        }
    }
}
