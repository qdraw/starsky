using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace starskytest.FakeMocks
{
	// Used by FakeMemoryCache
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
	public class FakeICacheEntry : ICacheEntry
	{
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}
		
		protected virtual void Dispose(bool _)
		{
			// do nothing
		}

		public object Key { get; } = string.Empty;
		public object? Value { get; set; }
		public DateTimeOffset? AbsoluteExpiration { get; set; }
		public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
		public TimeSpan? SlidingExpiration { get; set; }

		public IList<IChangeToken> ExpirationTokens { get; } =
			new List<IChangeToken>();

		public IList<PostEvictionCallbackRegistration>
			PostEvictionCallbacks { get; } =
			new List<PostEvictionCallbackRegistration>();
		
		public CacheItemPriority Priority { get; set; }
		public long? Size { get; set; }
	}
}
