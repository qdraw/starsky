using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;

namespace starsky.foundation.platform.Extensions
{
	public static class MemoryCacheExtensions
	{
		private static readonly Func<MemoryCache, object> GetEntriesCollection = Delegate.CreateDelegate(
			typeof(Func<MemoryCache, object>),
			typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true),
			throwOnBindFailure: true) as Func<MemoryCache, object>;

		private static IEnumerable GetKeys(this IMemoryCache memoryCache) =>
			((IDictionary)GetEntriesCollection((MemoryCache)memoryCache)).Keys;

		/// <summary>
		/// Get Keys
		/// </summary>
		/// <param name="memoryCache">memory cache</param>
		/// <typeparam name="T">bind as</typeparam>
		/// <returns>list of items</returns>
		public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache) {
			try
			{
				return GetKeys(memoryCache).OfType<T>();
			}
			catch ( InvalidCastException )
			{
				return new List<T>();
			}
		}
			
	}
}
