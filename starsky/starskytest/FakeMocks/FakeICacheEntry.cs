﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace starskytest.FakeMocks
{
	// Used by FakeMemoryCache
	public class FakeICacheEntry : ICacheEntry
	{
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing)
		{
			Dispose();
		}

		public object Key { get; }
		public object Value { get; set; }
		public DateTimeOffset? AbsoluteExpiration { get; set; }
		public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
		public TimeSpan? SlidingExpiration { get; set; }
		public IList<IChangeToken> ExpirationTokens { get; }
		public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; }
		public CacheItemPriority Priority { get; set; }
		public long? Size { get; set; }
	}
}
