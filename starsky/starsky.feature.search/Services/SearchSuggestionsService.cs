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
public class SearchSuggestionsService(
	ApplicationDbContext context,
	IMemoryCache? memoryCache,
	IWebLogger logger,
	AppSettings appSettings)
	: ISearchSuggest
{
	private const int MaxResult = 20;
	private const int InflateBatchSize = 5000;

	private sealed record TagBatchItem(int Id, string Tags);

	/// <summary>
	///     Used to fill the cache with an array of
	///     All keywords are stored lowercase
	/// </summary>
	/// <returns></returns>
	public async Task<List<KeyValuePair<string, int>>> Inflate()
	{
		if ( memoryCache == null )
		{
			return [];
		}

		if ( memoryCache.TryGetValue(nameof(SearchSuggestionsService), out _) )
		{
			return new Dictionary<string, int>().ToList();
		}

		try
		{
			var suggestions = await LoadSuggestionsAsync();
			var suggestionsFiltered = FilterSuggestions(suggestions);
			memoryCache.Set(nameof(SearchSuggestionsService), suggestionsFiltered,
				GetCacheExpire(suggestionsFiltered.Count));

			return suggestionsFiltered;
		}
		catch ( Exception exception )
		{
			if ( !exception.Message.Contains("Unknown column") )
			{
				logger.LogError(exception,
					$"[SearchSuggestionsService] exception catch-ed {exception.Message} {exception.StackTrace}");
			}

			return [];
		}
	}

	/// <summary>
	///     Cache query to get all stored suggested keywords
	/// </summary>
	/// <returns>Key/Value pared list</returns>
	public async Task<List<KeyValuePair<string, int>>> GetAllSuggestions()
	{
		if ( memoryCache == null || appSettings.AddMemoryCache == false )
		{
			return [];
		}

		if ( memoryCache.TryGetValue(nameof(SearchSuggestionsService),
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
		if ( string.IsNullOrEmpty(query) || memoryCache == null || appSettings.AddMemoryCache == false )
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

	private async Task<Dictionary<string, int>> LoadSuggestionsAsync()
	{
		var suggestions = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
		var lastId = 0;

		while ( true )
		{
			var currentLastId = lastId;
			var tagBatch = await context.FileIndex
				.AsNoTracking()
				.Where(p => p.Id > currentLastId && !string.IsNullOrEmpty(p.Tags))
				.OrderBy(p => p.Id)
				.Select(p => new TagBatchItem(p.Id, p.Tags!))
				.Take(InflateBatchSize)
				.TagWith("Inflate SearchSuggestionsService")
				.ToListAsync();

			if ( tagBatch.Count == 0 )
			{
				break;
			}

			foreach ( var item in tagBatch )
			{
				AddKeywords(suggestions, item.Tags);
			}

			lastId = tagBatch[^1].Id;
		}

		return suggestions;
	}

	private static void AddKeywords(Dictionary<string, int> suggestions, string tags)
	{
		var keywordsHashSet = HashSetHelper.StringToHashSet(tags.Trim());
		foreach ( var keyword in keywordsHashSet )
		{
			suggestions.TryGetValue(keyword, out var count);
			suggestions[keyword] = count + 1;
		}
	}

	private static List<KeyValuePair<string, int>> FilterSuggestions(
		Dictionary<string, int> suggestions)
	{
		return
		[
			.. suggestions
				.Where(p => p.Value >= 10)
				.OrderByDescending(p => p.Value)
		];
	}

	private static TimeSpan GetCacheExpire(int count)
	{
		// When changing here also change the cache expire time in SearchSuggestionsInflateHostedService
		return count != 0 ? new TimeSpan(120, 0, 0) : new TimeSpan(0, 1, 0);
	}
}
