using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Data;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.feature.search.Interfaces;

namespace starsky.feature.search.Services
{
	[Service(typeof(ISearchSuggest), InjectionLifetime = InjectionLifetime.Scoped)]
	public class SearchSuggestionsService : ISearchSuggest
	{
		private readonly ApplicationDbContext _context;
		private readonly IMemoryCache? _cache;
		private readonly AppSettings _appSettings;
		private readonly IWebLogger _logger;

		public SearchSuggestionsService(
			ApplicationDbContext context,
			IMemoryCache? memoryCache,
			IWebLogger logger,
			AppSettings appSettings)
		{
			_context = context;
			_cache = memoryCache;
			_logger = logger;
			_appSettings = appSettings;
		}

		private const int MaxResult = 20;

		/// <summary>
		/// Used to fill the cache with an array of
		/// All keywords are stored lowercase
		/// </summary>
		/// <returns></returns>
		[SuppressMessage("Performance",
			"CA1827:Do not use Count() or LongCount() when Any() can be used")]
		[SuppressMessage("Performance",
			"S1155:Do not use Count() or LongCount() when Any() can be used",
			Justification = "ANY is not supported by EF Core")]
		public async Task<List<KeyValuePair<string, int>>> Inflate()
		{
			if ( _cache == null )
			{
				return new List<KeyValuePair<string, int>>();
			}

			if ( _cache.TryGetValue(nameof(SearchSuggestionsService), out _) )
			{
				return new Dictionary<string, int>().ToList();
			}

			var allFilesList = new List<KeyValuePair<string, int>>();
			try
			{
				allFilesList = await _context.FileIndex
					.Where(p => !string.IsNullOrEmpty(p.Tags))
					.GroupBy(i => i.Tags)
					// ReSharper disable once UseMethodAny.1
					.Where(x => x.Count() >= 1) // .ANY is not supported by EF Core
					.TagWith("Inflate SearchSuggestionsService")
					.Select(val =>
						new KeyValuePair<string, int>(val.Key!, val.Count())).ToListAsync();
			}
			catch ( Exception exception )
			{
				if ( !exception.Message.Contains("Unknown column") )
				{
					_logger.LogError(exception,
						$"[SearchSuggestionsService] exception catch-ed {exception.Message} {exception.StackTrace}");
				}

				return allFilesList;
			}

			var suggestions =
				new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

			foreach ( var tag in allFilesList )
			{
				if ( string.IsNullOrEmpty(tag.Key) )
				{
					continue;
				}

				var keywordsHashSet = HashSetHelper.StringToHashSet(tag.Key.Trim());

				foreach ( var keyword in keywordsHashSet )
				{
					if ( suggestions.ContainsKey(keyword) )
					{
						suggestions[keyword] += tag.Value;
					}
					else
					{
						suggestions.Add(keyword, tag.Value);
					}
				}
			}

			var suggestionsFiltered = suggestions
				.Where(p => p.Value >= 10)
				.OrderByDescending(p => p.Value)
				.ToList();

			var cacheExpire = suggestionsFiltered.Count != 0
				? new TimeSpan(120, 0, 0)
				: new TimeSpan(0, 1, 0);

			_cache.Set(nameof(SearchSuggestionsService), suggestionsFiltered,
				cacheExpire);

			return suggestionsFiltered;
		}

		/// <summary>
		/// Cache query to get all stored suggested keywords
		/// </summary>
		/// <returns>Key/Value pared list</returns>
		public async Task<IEnumerable<KeyValuePair<string, int>>> GetAllSuggestions()
		{
			if ( _cache == null || _appSettings.AddMemoryCache == false )
			{
				return new Dictionary<string, int>();
			}

			if ( _cache.TryGetValue(nameof(SearchSuggestionsService),
					out var objectFileFolders) )
			{
				return objectFileFolders as List<KeyValuePair<string, int>> ??
					   new List<KeyValuePair<string, int>>();
			}

			return await Inflate();
		}

		/// <summary>
		/// Request is case-insensitive 
		/// </summary>
		/// <param name="query">half a search query</param>
		/// <returns>list of suggested keywords</returns>
		public async Task<IEnumerable<string>> SearchSuggest(string query)
		{
			if ( string.IsNullOrEmpty(query) )
			{
				return new List<string>();
			}

			if ( _cache == null || _appSettings.AddMemoryCache == false )
			{
				return new List<string>();
			}

			var allSuggestions = await GetAllSuggestions();

			var results = allSuggestions.Where(p =>
					p.Key.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
				.Take(MaxResult)
				.OrderByDescending(p => p.Value).Select(p => p.Key)
				.ToList();

			results.AddRange(SystemResults()
				.Where(p => p.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
				.Take(MaxResult));

			return results;
		}

		private static IEnumerable<string> SystemResults()
		{
			return
			[
				"-Datetime>7 -ImageFormat-\"tiff\"",
				"-ImageFormat:jpg",
				"-inUrl:",
				"-ImageFormat:gpx",
				"-ImageFormat:tiff",
				"-DateTime=1",
				"-fileHash:",
				"-filepath:",
				"-filename:",
				"-parentDirectory:",
				"-description",
				"-Datetime>12 -Datetime<2",
				"-addToDatabase: -Datetime>2",
				"-title:",
				"-isDirectory:false"
			];
		}
	}
}
