using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Performance.Contracts
{
    [ProtoContract]
    public class Sample
    {
        public Sample()
        {
            Values = new List<SampleData>();
        }

        [ProtoMember(1)]
        public Identity ID { get; set; }

        [ProtoMember(2)]
        public int SampleType { get; set; }

        [ProtoMember(3)]
        public ICollection<SampleData> Values { get; set; }
    }

    [ProtoContract]
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

        [ProtoMember(1)]
        public int Catalog { get; set; }

        [ProtoMember(2)]
        public long ID { get; set; }

    }

    [ProtoContract]
    public class SampleData
    {
        [ProtoMember(1)]
        public DataType DataType { get; set; }
        
        [ProtoMember(2)]
        public decimal Value { get; set; }
        
        [ProtoMember(3)]
        public decimal Threshold { get; set; }
    }

    [ProtoContract]
    public enum DataType
    {
        [ProtoEnum]
        Unknown = 0,
        [ProtoEnum]
        Percentage = 1,
        [ProtoEnum]
        Dollar = 2
    }
}
