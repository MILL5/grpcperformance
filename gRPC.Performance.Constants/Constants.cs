using System;

namespace gRPC.Performance
{
    public static class Constants
    {
        public const int BatchSize = 1;

        public static int Parts
        {
            get
            {
                return BatchSize >= 5 ? 5 : 1;
            }
        }
    }
}
