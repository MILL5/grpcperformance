using System;
using System.Text.Json.Serialization;

namespace Performance
{

    public class VersionInfo
    {
        [JsonPropertyName("sid")]
        public Guid ServerId { get; set; }
        [JsonPropertyName("v")]
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
        None,
        Initial,
        VersionUpdate,
        ServerMigration
    }

    public class VersionedResponse<T> where T: class
    {
        [JsonPropertyName("u")]
        public VersionUpdate Update { get; set; }

        [JsonPropertyName("vi")]
        public VersionInfo Version { get; set; }

        [JsonPropertyName("v")]
        public T Value { get; set; }
    }
}
