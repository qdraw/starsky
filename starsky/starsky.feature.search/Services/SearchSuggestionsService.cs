using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.feature.search.Interfaces;
using starsky.foundation.database.Data;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.feature.search.Services;

[Service(typeof(ISearchSuggest), InjectionLifetime = InjectionLifetime.Scoped)]
public class SearchSuggestionsService : ISearchSuggest
{
	private const int MaxResult = 20;
	private readonly AppSettings _appSettings;
	private readonly IMemoryCache? _cache;
	private readonly ApplicationDbContext _context;
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

	/// <summary>
	///     Used to fill the cache with an array of
	///     All keywords are stored lowercase
	/// </summary>
	/// <returns></returns>
	public async Task<List<KeyValuePair<string, int>>> Inflate()
	{
		if ( _cache == null )
		{
			return [];
		}

		if ( _cache.TryGetValue(nameof(SearchSuggestionsService), out _) )
		{
			return new Dictionary<string, int>().ToList();
		}

		// Select only Tags and stream via AsAsyncEnumerable so EF Core's internal buffer
		// holds one column instead of full rows, and no extra List<string> is allocated.
		var suggestions = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
		try
		{
			await foreach ( var tagsString in _context.FileIndex
				               .AsNoTracking()
				               .Where(p => !string.IsNullOrEmpty(p.Tags))
				               .Select(p => p.Tags!)
				               .TagWith("Inflate SearchSuggestionsService")
				               .AsAsyncEnumerable() )
			{
				if ( string.IsNullOrEmpty(tagsString) )
				{
					continue;
				}

				foreach ( var keyword in HashSetHelper.StringToHashSet(tagsString.Trim()) )
				{
					suggestions.TryGetValue(keyword, out var count);
					suggestions[keyword] = count + 1;
				}
			}
		}
		catch ( Exception exception )
		{
			if ( !exception.Message.Contains("Unknown column") )
			{
				_logger.LogError(exception,
					$"[SearchSuggestionsService] exception catch-ed {exception.Message} {exception.StackTrace}");
			}

			return [];
		}

		var suggestionsFiltered = suggestions
			.Where(p => p.Value >= 10)
			.OrderByDescending(p => p.Value)
			.ToList();

		// When changing here also change the cache expire time in SearchSuggestionsInflateHostedService
		var cacheExpire = suggestionsFiltered.Count != 0
			? new TimeSpan(120, 0, 0)
			: new TimeSpan(0, 1, 0);

		_cache.Set(nameof(SearchSuggestionsService), suggestionsFiltered,
			cacheExpire);

		return suggestionsFiltered;
	}

	/// <summary>
	///     Cache query to get all stored suggested keywords
	/// </summary>
	/// <returns>Key/Value pared list</returns>
	public async Task<List<KeyValuePair<string, int>>> GetAllSuggestions()
	{
		if ( _cache == null || _appSettings.AddMemoryCache == false )
		{
			return [];
		}

		if ( _cache.TryGetValue(nameof(SearchSuggestionsService),
			    out var objectFileFolders) )
		{
			return objectFileFolders as List<KeyValuePair<string, int>> ??
			       [];
		}

		return await Inflate();
	}

	/// <summary>
	///     Request is case-insensitive
	/// </summary>
	/// <param name="query">half a search query</param>
	/// <param name="system">search queries</param>
	/// <returns>list of suggested keywords</returns>
	public async Task<IEnumerable<string>> SearchSuggest(string query, bool system)
	{
		if ( string.IsNullOrEmpty(query) || _cache == null || _appSettings.AddMemoryCache == false )
		{
			return new List<string>();
		}

		// do not modify allSuggestions, because it is stored in cache
		var allSuggestions = await GetAllSuggestions();

		var results = allSuggestions.Where(p =>
				p.Key.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
			.Take(MaxResult)
			.OrderByDescending(p => p.Value).Select(p => p.Key)
			.ToList();

		if ( system )
		{
			results.AddRange(SystemResults().Where(p =>
					p.StartsWith(query, StringComparison.InvariantCultureIgnoreCase))
				.Take(MaxResult));
		}

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
