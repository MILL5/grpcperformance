using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Performance
{
    public class Sample
    {
        public Sample()
        {
            Values = new List<SampleData>();
        }

        [JsonPropertyName("id")]
        public Identity ID { get; set; }

        [JsonPropertyName("st")]
        public int SampleType { get; set; }

        [JsonPropertyName("v")]
        public ICollection<SampleData> Values { get; set; }
    }

    public class Identity
    {
        public override bool Equals(object obj)
        {
            if (obj is Identity other)
            {
                return Catalog == other.Catalog && ID == other.ID;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Catalog, ID);
        }

        [JsonPropertyName("c")]
        public int Catalog { get; set; }
        [JsonPropertyName("id")]
        public long ID { get; set; }
    }

    public class SampleData
    {
        [JsonPropertyName("dt")]
        public DataType DataType { get; set; }
        [JsonPropertyName("v")]
        public decimal Value { get; set; }
        [JsonPropertyName("t")]
        public decimal Threshold { get; set; }
    }

    public enum DataType
    {
        Unknown = 0,
        Percentage = 1,
        Dollar = 2
    }
}
