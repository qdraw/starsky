using System;
using System.Collections.Generic;
using System.Linq;

namespace starskycore.Helpers
{
	// Type: System.Linq.Enumerable
	// Assembly: System.Linq, Version=4.2.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
	// MVID: EF3FAE8D-2834-4F19-ADF9-4E2F673C5F56
	// Assembly location: /usr/local/share/dotnet/shared/Microsoft.NETCore.App/2.2.0/System.Linq.dll
	public static class SkipLastHelper
	{

		public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
		{
			if (source == null)
				throw new ArgumentNullException(nameof (source));
			if (count <= 0)
				return source.Skip<TSource>(0);
			return SkipLastIterator<TSource>(source, count);
		}

		private static IEnumerable<TSource> SkipLastIterator<TSource>(IEnumerable<TSource> source, int count)
		{
			Queue<TSource> queue = new Queue<TSource>();
			using (IEnumerator<TSource> e = source.GetEnumerator())
			{
				while (e.MoveNext())
				{
					if (queue.Count == count)
					{
						do
						{
							yield return queue.Dequeue();
							queue.Enqueue(e.Current);
						}
						while (e.MoveNext());
						break;
					}
					queue.Enqueue(e.Current);
				}
			}
		}

	}
}
