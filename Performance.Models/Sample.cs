using System;
using System.Collections.Generic;

namespace Performance.Models
{
    public class Sample
    {
        public Sample()
        {
            Values = new List<SampleData>();
        }

        public Identity ID { get; set; }

        public int SampleType { get; set; }

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

        public int Catalog { get; set; }
        public long ID { get; set; }
    }

    public class SampleData
    {
        public DataType DataType { get; set; }
        public decimal Value { get; set; }
        public decimal Threshold { get; set; }
    }

    public enum DataType
    {
        Unknown = 0,
        Percentage = 1,
        Dollar = 2
    }
}
