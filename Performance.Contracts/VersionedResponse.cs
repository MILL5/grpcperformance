﻿using ProtoBuf;
using System;

namespace Performance
{

    [ProtoContract(Name = "vi")]
    public class VersionInfo
    {
        [ProtoMember(1, Name = "sid")]
        public Guid ServerId { get; set; }
        [ProtoMember(2, Name = "v")]
        public int Version { get; set; }

        public static VersionInfo ToVersionInfo(Guid? serverId, int? version)
        {
            if (!serverId.HasValue || !version.HasValue)
            {
                return null;
            }

            return new VersionInfo { ServerId = serverId.Value, Version = version.Value };
        }
    }

    public enum VersionUpdate
    {
        [ProtoEnum(Name = "n")]
        None,
        [ProtoEnum(Name = "i")]
        Initial,
        [ProtoEnum(Name = "vu")]
        VersionUpdate,
        [ProtoEnum(Name = "sm")]
        ServerMigration
    }

    [ProtoContract(Name = "vr")]
    public class VersionedResponse<T> where T: class
    {
        [ProtoMember(1, Name = "u")]
        public VersionUpdate Update { get; set; }

        [ProtoMember(2, Name = "vi")]
        public VersionInfo Version { get; set; }

        [ProtoMember(3, Name = "v")]
        public T Value { get; set; }
    }
}
