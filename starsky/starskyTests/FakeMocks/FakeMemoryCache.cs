using Microsoft.Extensions.Caching.Memory;
using starsky.Models;

namespace starskytests.FakeMocks
{
    public class FakeMemoryCache :IMemoryCache
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(object key, out object value)
        {
            value = new FileIndexItem{Tags = "test"};
            return true;
        }

        public ICacheEntry CreateEntry(object key)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(object key)
        {
        }
    }
}