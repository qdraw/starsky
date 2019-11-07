using System;
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
		/// All keywords are stored lowercase
		/// </summary>
		/// <returns></returns>
		public List<KeyValuePair<string,int>> Inflate()
		{
			if (_cache.TryGetValue(nameof(SearchSuggestionsService), 
				out _)) return new Dictionary<string,int>().ToList();

			var suggestions = new Dictionary<string,int>();
			
			var allFilesList = _context.FileIndex.ToList();

			foreach ( var file in allFilesList )
			{
				if(string.IsNullOrEmpty(file.Tags)) continue;

				foreach ( var keywordCase in file.Keywords )
				{
					var keyword = keywordCase.ToLowerInvariant();
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
				new TimeSpan(100,0,0));

			return suggestionsFiltered;
		}

		/// <summary>
		/// Cache query to get all stored suggested keywords
		/// </summary>
		/// <returns>Key/Value pared list</returns>
		public IEnumerable<KeyValuePair<string, int>> GetAllSuggestions()
		{
			if( _cache == null || _appSettings?.AddMemoryCache == false) 
				return new Dictionary<string,int>();
			
			if (_cache.TryGetValue(nameof(SearchSuggestionsService), out var objectFileFolders))
				return objectFileFolders as List<KeyValuePair<string,int>>;
			
			return Inflate();
		
		}

		/// <summary>
		/// Request is case-insensitive 
		/// </summary>
		/// <param name="query">half a search query</param>
		/// <returns>list of suggested keywords</returns>
		public IEnumerable<string> SearchSuggest(string query)
		{
			if ( string.IsNullOrEmpty(query) ) return new List<string>();
			if( _cache == null || _appSettings?.AddMemoryCache == false) return new List<string>();
			
			var results = GetAllSuggestions().Where(p => p.Key.ToLowerInvariant().StartsWith( query.ToLowerInvariant() )).Take(MaxResult)
				.OrderByDescending(p => p.Value).Select(p => p.Key).ToList();
			
			results.AddRange(SystemResults().Where(p => p.ToLowerInvariant().StartsWith(query.ToLowerInvariant()))
				.Take(MaxResult) );
			return results;
		}

		private IEnumerable<string> SystemResults()
		{
			return new HashSet<string>
			{
				"-Datetime>7 -ImageFormat-\"tiff\"",
				"-ImageFormat:jpg",
				"-inurl:",
				"-ImageFormat:gpx",
				"-ImageFormat:tiff",
				"-DateTime=1",
				"-fileHash:",
				"-filepath:",
				"-filename:",
				"-parentDirectory:",
				"-description",
				"-Datetime>12 -Datetime<2",
				"-addtodatabase: -Datetime>2",
				"-title:",
				"-isdirectory:false"
			};
		}

	}
}
