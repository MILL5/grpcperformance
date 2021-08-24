using Microsoft.VisualStudio.TestTools.UnitTesting;
using Performance.Cache;
using Shouldly;

namespace Performance.UnitTests
{
    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        public void BuildCacheShouldNotBeNullTest()
        {
            var cache = CacheInstance.GetCache();

            cache.ShouldNotBeNull();
            cache.Count.ShouldBe(CacheInstance.CacheSize);
        }
    }
}
