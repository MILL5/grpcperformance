using System;
using System.Collections.Generic;
using System.Linq;
using static Pineapple.Common.Preconditions;

namespace gRPC
{
    public static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            CheckIsNotNull(nameof(list), list);
            CheckIsNotLessThanOrEqualTo(nameof(parts), parts, 0);

            int count = list.Count();
            int batchSize = Math.Max(Convert.ToInt32(Math.Floor(count * 1.0 / parts)),1);

            var batches = new HashSet<IEnumerable<T>>(parts);
            var currentBatch = new List<T>(batchSize);

            foreach (var item in list)
            {
                currentBatch.Add(item);

                if (currentBatch.Count == batchSize)
                {
                    batches.Add(currentBatch);
                    currentBatch = new List<T>(batchSize);
                }
            }

            if (currentBatch.Count > 0 && currentBatch.Count != batchSize)
            {
                batches.Add(currentBatch);
            }

            return batches;
        }
    }
}
