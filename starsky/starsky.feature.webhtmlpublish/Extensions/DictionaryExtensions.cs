using System;
using System.Collections.Generic;

namespace starsky.feature.webhtmlpublish.Extensions
{
	/// <summary>
	/// Helpers for Dictionaries
	/// @see: https://stackoverflow.com/a/28942210
	/// </summary>
	public static class DictionaryExtensions
	{
		public static void AddRangeOverride<TKey, TValue>(this IDictionary<TKey, TValue> dic, IDictionary<TKey, TValue> dicToAdd)
		{
			dicToAdd.ForEach(x => dic[x.Key] = x.Value);
		}
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
				action(item);
		}
	}
}
