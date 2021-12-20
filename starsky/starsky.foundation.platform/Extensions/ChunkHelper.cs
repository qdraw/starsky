using System.Collections.Generic;
using System.Linq;

namespace starsky.foundation.platform.Extensions
{
	public static class ChunkHelper
	{
		/// <summary>
		/// Break a list of items into chunks of a specific size
		/// </summary>
		public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
		{
			if ( source == null ) yield break;
			// ReSharper disable once PossibleMultipleEnumeration
			while ( source.Any())
			{
				// ReSharper disable once PossibleMultipleEnumeration
				yield return source.Take(chunkSize);
				// ReSharper disable once PossibleMultipleEnumeration
				source = source.Skip(chunkSize);
			}

		}
	}
}
