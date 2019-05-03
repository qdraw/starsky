using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
	public class SearchSuggestionsService : ISearchSuggest
	{
		private readonly ApplicationDbContext _context;
		private readonly IMemoryCache _cache;
		private readonly AppSettings _appSettings;

		public SearchSuggestionsService(
			ApplicationDbContext context, 
			IMemoryCache memoryCache = null,
			AppSettings appSettings = null)
		{
			_context = context;
			_cache = memoryCache;
			_appSettings = appSettings;
		}

		public List<string> SearchSuggest(string query)
		{
			return _context.FileIndex.Select(p => p.Tags).ToList();
		}
	}
}
