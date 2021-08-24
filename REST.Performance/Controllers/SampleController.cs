﻿using Microsoft.AspNetCore.Mvc;
using Performance.Models;
using System;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace REST.Performance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private static readonly Random _random = new();

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

        private static Sample GetSample()
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

        [HttpGet("{catalog}/{id}")]
        public async ValueTask<Sample> Get(int catalog, long id)
        {
            var result = await ValueTask.FromResult(GetSample());
            return result;
        }
    }
}
