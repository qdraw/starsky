using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using starskycore.Data;
using starskycore.Helpers;
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
			IMemoryCache memoryCache,
			AppSettings appSettings = null)
		{
			_context = context;
			_cache = memoryCache;
			_appSettings = appSettings;
		}

		private const int MaxResult = 20;

		/// <summary>
		/// Used to fill the cache with an array of
		/// </summary>
		/// <returns></returns>
		public List<KeyValuePair<string,int>> Populate()
		{
			if (_cache.TryGetValue(nameof(SearchSuggestionsService), 
				out _)) return new Dictionary<string,int>().ToList();

			var suggestions = new Dictionary<string,int>();
			
			var allFilesList = _context.FileIndex.ToList();

			foreach ( var file in allFilesList )
			{
				if(string.IsNullOrEmpty(file.Tags)) continue;

				foreach ( var keyword in file.Keywords )
				{
					if ( suggestions.ContainsKey(keyword) )
					{
						suggestions[keyword]++;
					}
					else
					{
						suggestions.Add(keyword,1);
					}
				}
			}

			var suggestionsFiltered = suggestions.Where(p => p.Value >= 10).ToList();
			
			_cache.Set(nameof(SearchSuggestionsService), suggestionsFiltered, 
				new TimeSpan(20,0,0));

			return suggestionsFiltered;
		}

		private IEnumerable<KeyValuePair<string, int>> GetAllSuggestions()
		{
			if( _cache == null || _appSettings?.AddMemoryCache == false) return new Dictionary<string,int>();
			
			if (_cache.TryGetValue(nameof(SearchSuggestionsService), out var objectFileFolders))
				return objectFileFolders as List<KeyValuePair<string,int>>;
			
			return Populate();
		
		}

		public IEnumerable<string> SearchSuggest(string query)
		{
			if ( string.IsNullOrEmpty(query) ) return new List<string>();
			if( _cache == null || _appSettings?.AddMemoryCache == false) return new List<string>();

			return GetAllSuggestions().Where(p => p.Key.StartsWith(query)).Take(MaxResult)
				.OrderByDescending(p => p.Value).Select(p => p.Key);
		}

	}
}
