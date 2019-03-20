using Microsoft.Extensions.Caching.Memory;
using starskycore.Interfaces;

namespace starskycore.Services
{
	public class ReadMetaExif
	{
		private IMemoryCache _cache;
		private IStorage _iStorage;

		public ReadMetaExif(IStorage iStorage, IMemoryCache memoryCache = null)
		{
			_cache = memoryCache;
			_iStorage = iStorage;
		}
	}
}
