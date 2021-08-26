using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Performance
{
    [ProtoContract(Name = "s")]
    public class Sample
    {
        public Sample()
        {
            Values = new List<SampleData>();
        }

        [ProtoMember(1, Name = "id")]
        public Identity ID { get; set; }

        [ProtoMember(2, Name = "st")]
        public int SampleType { get; set; }

        [ProtoMember(3, Name = "v")]
        public ICollection<SampleData> Values { get; set; }
    }

    [ProtoContract(Name = "id")]
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

        [ProtoMember(1, Name = "c")]
        public int Catalog { get; set; }

        [ProtoMember(2, Name = "id")]
        public long ID { get; set; }

    }

    [ProtoContract(Name = "sd")]
    public class SampleData
    {
        [ProtoMember(1, Name = "dt")]
        public DataType DataType { get; set; }
        
        [ProtoMember(2, Name = "v")]
        public decimal Value { get; set; }
        
        [ProtoMember(3, Name = "t")]
        public decimal Threshold { get; set; }
    }

    [ProtoContract(Name = "dt")]
    public enum DataType
    {
        [ProtoEnum(Name = "u")]
        Unknown = 0,
        [ProtoEnum(Name = "p")]
        Percentage = 1,
        [ProtoEnum(Name = "d")]
        Dollar = 2
    }
}
