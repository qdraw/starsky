using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.feature.search.Interfaces;
using starsky.feature.search.ViewModels;
#nullable enable

namespace starsky.feature.search.Services
{
	[Service(typeof(ISearch), InjectionLifetime = InjectionLifetime.Scoped)]
	public class SearchService : ISearch
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache? _cache;
        private readonly AppSettings? _appSettings;
        private readonly IWebLogger _logger;

        public SearchService(
            ApplicationDbContext context, 
            IWebLogger logger,
            IMemoryCache? memoryCache = null,
            AppSettings? appSettings = null)
        {
            _context = context;
            _cache = memoryCache;
            _appSettings = appSettings;
            _logger = logger;
        }

        /// <summary>
        /// Search in database
        /// </summary>
        /// <param name="query">where to search in</param>
        /// <param name="pageNumber">current page (0 = page 1)</param>
        /// <param name="enableCache">enable searchcache (in trash this is disabled) </param>
        /// <returns></returns>
        public async Task<SearchViewModel> Search(string query = "",
	        int pageNumber = 0, bool enableCache = true)
	    {
		    if ( !string.IsNullOrEmpty(query) && query.Length >= 500 )
		    {
			    throw new ArgumentException("Search Input Query is longer then 500 chars");
		    }

		    if ( query == TrashKeyword.TrashKeywordString )
		    {
			    _logger.LogInformation("Skip cache for trash");
			    enableCache = false;
		    }

		    if ( !enableCache ||
		         _cache == null || _appSettings?.AddMemoryCache == false )
		    {
			    return SkipSearchItems(await SearchDirect(query),pageNumber);
		    }

            // Return values from IMemoryCache
            var querySearchCacheName = "search-" + query;
            
            // Return Cached object if it exist
            if ( _cache.TryGetValue(querySearchCacheName,
	                out var objectSearchModel) )
            {
	            return SkipSearchItems(objectSearchModel, pageNumber);
            }
            
            // Try to catch a new object
            objectSearchModel = await SearchDirect(query);
            _cache.Set(querySearchCacheName, objectSearchModel, new TimeSpan(0,10,0));
            return SkipSearchItems(objectSearchModel, pageNumber);
        }
	    
	    public bool? RemoveCache(string query)
	    {
		    // Add protection for disabled caching
		    if( _cache == null || _appSettings?.AddMemoryCache == false) return null;
            
		    var queryCacheName = "search-" + query;
		    if (!_cache.TryGetValue(queryCacheName, out _)) return false;
		    _cache.Remove(queryCacheName);
		    return true;
	    }
	    
	    /// <summary>
        /// Skip un-needed items
        /// </summary>
        /// <param name="objectSearchModel"></param>
        /// <param name="pageNumber">current page (0 = page 1)</param>
        /// <returns></returns>
        private static SearchViewModel SkipSearchItems(object? objectSearchModel, int pageNumber)
        {
	        if ( objectSearchModel is not SearchViewModel searchModel)
            {
	            return new SearchViewModel();
            }
        
	        // Clone the item to avoid removing items from cache
            searchModel = searchModel.Clone();
            if ( searchModel.FileIndexItems == null )
            {
	            return new SearchViewModel();
            }
            
            searchModel.PageNumber = pageNumber;
	        
	        var skipFirstNumber = pageNumber * NumberOfResultsInView;
	        var skipLastNumber = searchModel.SearchCount - ( pageNumber * NumberOfResultsInView ) - NumberOfResultsInView;

	        
	        // Remove the last items
	        var skippedLastList = searchModel.FileIndexItems
		        .Skip(skipFirstNumber)
		        .SkipLast(skipLastNumber);

	        var skippedHashSet = new HashSet<FileIndexItem>(skippedLastList);
	        searchModel.FileIndexItems = skippedHashSet.ToList();

	        return searchModel;
        }

        /// <summary>
        /// Return all results
        /// </summary>
        /// <param name="query">where to search on</param>
        /// <returns></returns>
        private async Task<SearchViewModel> SearchDirect(string? query = "")
        {
            var stopWatch = Stopwatch.StartNew();

            // Create an view model
            var model = new SearchViewModel
            {
                SearchQuery = query ?? string.Empty,
                Breadcrumb = new List<string> {"/", query ?? string.Empty  }
                // Null check will safe you from error 500 with Empty request
            };

            if ( query == null || model.FileIndexItems == null )
            {
	            return model;
            }

            _orginalSearchQuery = model.SearchQuery;

            model.SearchQuery = QuerySafe(model.SearchQuery);
            model.SearchQuery = QueryShortcuts(model.SearchQuery);
            model = MatchSearch(model);

            model = await WideSearch(_context.FileIndex.AsNoTracking(),model);
	        
            model = SearchViewModel.NarrowSearch(model);

            // Remove duplicates from list
            model.FileIndexItems = model.FileIndexItems!
	            .Where(p => p.FilePath != null)
	            .GroupBy(s => s.FilePath)
                .Select(grp => grp.FirstOrDefault())
                .OrderBy(s => s!.FilePath)
                .ToList()!;
            
            model.SearchCount = model.FileIndexItems.Count;

            model.FileIndexItems = model.FileIndexItems
                .OrderByDescending(p => p.DateTime).ToList();

            model.LastPageNumber = GetLastPageNumber(model.SearchCount);

            model.ElapsedSeconds = stopWatch.Elapsed.TotalSeconds;
            return model;
        }

	    /// <summary>
	    /// Main method to query the database, in other function there is sorting needed
	    /// </summary>
	    /// <param name="sourceList">IQueryable database</param>
	    /// <param name="model">temp output model</param>
	    /// <returns>search model with content</returns>
	    [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
	    [SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' " +
	                                    "method overloads to perform case-insensitive string comparisons", 
		    Justification = "EF Core does not support this:  System.InvalidOperationException: The LINQ expression 'DbSet<FileIndexItem>()")]
	    private async Task<SearchViewModel> WideSearch(IQueryable<FileIndexItem> sourceList,
		    SearchViewModel model)
	    {
		    var predicates = new List<Expression<Func<FileIndexItem,bool>>>();  

		    // .AsNoTracking() => never change data to update
		    for ( var i = 0; i < model.SearchIn.Count; i++ )
		    {
				Enum.TryParse<SearchViewModel.SearchInTypes>(model.SearchIn[i].ToLowerInvariant(), 
					true, out var searchInType);

			    if ( model.SearchForOptions[i] == SearchViewModel.SearchForOptionType.Not )
			    {
				    continue;
			    }
			    
			    switch ( searchInType )
			    {
				    case SearchViewModel.SearchInTypes.imageformat:
					    if ( Enum.TryParse<ExtensionRolesHelper.ImageFormat>(
						        model.SearchFor[i].ToLowerInvariant(), out var castImageFormat) )
					    {
						    var result = castImageFormat;
						    predicates.Add(x => x.ImageFormat == result);
					    }
					    break;
				    case SearchViewModel.SearchInTypes.description:
						// need to have description out of the Func<>
						// ToLowerInvariant.Contains(__description_1))' could not be translated. 
					    var description = model.SearchFor[i];
					    predicates.Add(x => x.Description!.ToLower().Contains(description));
					    break;
				    case SearchViewModel.SearchInTypes.filename:
					    var filename = model.SearchFor[i];
					    predicates.Add(x => x.FileName!.ToLower().Contains(filename));
					    break;
				    case SearchViewModel.SearchInTypes.filepath:
					    var filePath = model.SearchFor[i];
					    predicates.Add(x => x.FilePath!.ToLower().Contains(filePath));
					    break;
				    case SearchViewModel.SearchInTypes.parentdirectory:
					    var parentDirectory = model.SearchFor[i];
					    predicates.Add(x => x.ParentDirectory!.ToLower().Contains(parentDirectory));
					    break;
				    case SearchViewModel.SearchInTypes.title:
					    var title = model.SearchFor[i];
					    predicates.Add(x => x.Title!.ToLower().Contains(title));
					    break;
				    case SearchViewModel.SearchInTypes.make:
					    // is in the database one field => will be filtered in narrowSearch
					    var make = model.SearchFor[i];
					    predicates.Add(x => x.MakeModel!.ToLower().Contains(make));
					    break;
				    case SearchViewModel.SearchInTypes.model:
					    // is in the database one field => will be filtered in narrowSearch
					    var modelMake = model.SearchFor[i];
					    predicates.Add(x => x.MakeModel!.ToLower().Contains(modelMake));
					    break;
				    case SearchViewModel.SearchInTypes.filehash:
					    var fileHash = model.SearchFor[i];
					    predicates.Add(x => x.FileHash != null && x.FileHash.ToLower().Contains(fileHash) );
					    break;
				    case SearchViewModel.SearchInTypes.software:
					    var software = model.SearchFor[i];
					    predicates.Add(x => x.Software!.ToLower().Contains(software));
					    break;
				    case SearchViewModel.SearchInTypes.isdirectory:
					    if ( bool.TryParse(model.SearchFor[i].ToLowerInvariant(),
						        out var boolIsDirectory) )
					    {
						    predicates.Add(x => x.IsDirectory == boolIsDirectory);
						    model.SearchFor[i] = boolIsDirectory.ToString();
					    }
					    break;
				    case SearchViewModel.SearchInTypes.lastedited:
					    predicates.Add(SearchWideDateTime.WideSearchDateTimeGet(model,i,SearchWideDateTime.WideSearchDateTimeGetType.LastEdited));
					    break;
				    case SearchViewModel.SearchInTypes.addtodatabase:
					    predicates.Add(SearchWideDateTime.WideSearchDateTimeGet(model,i,SearchWideDateTime.WideSearchDateTimeGetType.AddToDatabase));
					    break;
				    case SearchViewModel.SearchInTypes.datetime:
					    predicates.Add(SearchWideDateTime.WideSearchDateTimeGet(model,i,SearchWideDateTime.WideSearchDateTimeGetType.DateTime));
					    break;
				    case SearchViewModel.SearchInTypes.colorclass:
					    if ( Enum.TryParse<ColorClassParser.Color>(
						        model.SearchFor[i].ToLowerInvariant(), out var castColorClass) )
					    {
						    predicates.Add(x => x.ColorClass == castColorClass);
					    }
					    break;
				    default:
					    var tags = model.SearchFor[i];
					    predicates.Add(x => x.Tags!.ToLower().Contains(tags));
					    break;
			    }
			    // Need to have the type registered in FileIndexPropList
		    }
		    
		    _logger.LogInformation($"search --> {model.SearchQuery}");

		    var predicate = PredicateExecution(predicates, model);
		    
		    model.FileIndexItems = await sourceList.Where(predicate).ToListAsync();
		    
		    return model;
	    }
	    private static Expression<Func<FileIndexItem, bool>> PredicateExecution(
		    IReadOnlyList<Expression<Func<FileIndexItem, bool>>> predicates,
		    SearchViewModel model)
	    {
		    var predicate = PredicateBuilder.False<FileIndexItem>();
		    for ( var i = 0; i < predicates.Count; i++ )
		    {
			    if ( i == 0 )
			    {
				    predicate = predicates[i];
				    continue;
			    }
			    
			    var item = predicates[i - 1];
			    var item2 = predicates[i];
				    
			    // Search for OR
			    if ( !model.SearchOperatorContinue(i, model.SearchIn.Count) )
			    {
				    predicate =  item.Or(item2);
				    continue;
			    }

			    predicate =  item.AndAlso(item2);
		    }

		    return predicate;
	    }

	    /// <summary>
		/// Store the query during search
		/// </summary>
        private string _defaultQuery = string.Empty;
	    
	    /// <summary>
	    /// The orginal user search query
	    /// </summary>
        private string _orginalSearchQuery = string.Empty;

	    /// <summary>
	    /// Parse search query for -Tags and default search queries e.g. "test"
	    /// </summary>
	    /// <param name="model">Search model</param>
	    /// <returns>filled fields in model</returns>
        public SearchViewModel MatchSearch(SearchViewModel model)
        {
	        // return nulls to avoid errors
	        if ( string.IsNullOrWhiteSpace(model.SearchQuery) )
	        {
		        return model;
	        }

	        _defaultQuery = model.SearchQuery;

	        // Need to have the type registered in FileIndexPropList
	        
            foreach (var itemName in FileIndexItem.FileIndexPropList())
            {
                SearchItemName(model, itemName);
            }
	        
			// handle keywords without for example -Tags, or -DateTime prefix
	        model.ParseDefaultOption(_defaultQuery);
	        
            model.SearchQuery = _orginalSearchQuery;
            return model;
        }

	    /// <summary>
	    /// Search for e.g. -Tags:"test"
	    /// </summary>
	    /// <param name="model">Model</param>
	    /// <param name="itemName">e.g. Tags or Description</param>
	    private void SearchItemName(SearchViewModel model, string itemName)
        {
	        if ( model.SearchQuery == null ) return;

	        // ignore double quotes
	        model.SearchQuery = model.SearchQuery.Replace("\"\"", "\"");
	        
	        // Escape special quotes
	        model.SearchQuery = Regex.Replace(model.SearchQuery, "[“”‘’]", "\"", 
		        RegexOptions.None, TimeSpan.FromMilliseconds(100));

	        // new: unescaped
	        // (:|=|;|>|<|-)((["'])(\\?.)*?\3|[\w\!\~\-_\.\/:,;]+)( \|\|| \&\&)?
            Regex inurlRegex = new Regex(
                "-" + itemName +
                "(:|=|;|>|<|-)(([\"\'])(\\\\?.)*?\\3|[\\w\\!\\~\\-_\\.\\/:,;]+)( \\|\\|| \\&\\&)?",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
	        
            _defaultQuery = inurlRegex.Replace(_defaultQuery,"");
	        // the current query is removed from the list, so the next item will not search on it

            var regexInUrlMatches = inurlRegex.Matches(model.SearchQuery);
            if(regexInUrlMatches.Count == 0) return;

            foreach (var regexInUrlValue in regexInUrlMatches.Select(p => p.Value))
            {
                var itemQuery = regexInUrlValue;
	            
	            // ignore fake results
	            if ( string.IsNullOrEmpty(itemQuery) ) continue;

	            // put ||&& in operator field => next regex > removed
	            var itemQueryWithOperator = itemQuery;
	            
	            Regex rgx = new Regex("-"+ itemName +"(:|=|;|>|<|-)", 
		            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

                // To Search Type
                var itemNameSearch = rgx.Match(itemQuery).Value;
                if (itemNameSearch.Length == 0 ) continue;

                // replace
                itemQuery = rgx.Replace(itemQuery, string.Empty);

	            // Option last of itemNameSearch (Get last option = ^1)
                var searchForOption = itemNameSearch[^1].ToString();
                
                // Remove parenthesis
                itemQuery = itemQuery.Replace("\"", string.Empty);
                itemQuery = itemQuery.Replace("'", string.Empty);

	            // Remove || / && at the end of the string
	            // (\|\||\&\&)$
	            const string pattern = "(\\|\\||\\&\\&)$";
				itemQuery = Regex.Replace(itemQuery, pattern, string.Empty, 
					RegexOptions.None, TimeSpan.FromMilliseconds(100));
	            
				// is | or &
				var andOrChar = SearchViewModel.AndOrRegex(itemQueryWithOperator);

				var splitItemQueryArray = itemQuery.Split(',');
				foreach ( var itemSingleQuery in splitItemQueryArray )
				{
					var searchForOptionEnum = model.SetAddSearchForOptions(searchForOption);
					var andOrCharOperator =
						itemQuery.Contains(',') ? '|' : andOrChar;
					if ( searchForOptionEnum == SearchViewModel.SearchForOptionType.Not )
					{
						andOrCharOperator = '&';
					}
					model.SetAndOrOperator(andOrCharOperator);
					model.SetAddSearchFor(itemSingleQuery.Trim());
					model.SetAddSearchInStringType(itemName);
				}
            }
        }

	    /// <summary>
	    /// Trim value (remove spaces)
	    /// </summary>
	    /// <param name="query">searchQuery</param>
	    /// <returns>trimmed value</returns>
        public static string QuerySafe(string query)
        {
            query = query.Trim();
            return query;
        }

	    /// <summary>
	    /// Allow -inurl shortcut
	    /// </summary>
	    /// <param name="query">search Query</param>
	    /// <returns>replaced Url</returns>
        public static string QueryShortcuts(string query)
        {
	        // should be ignoring case
            query = Regex.Replace(query, "-inurl", "-FilePath", 
	            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            return query;
        }

	    /// <summary>
	    /// The amount of search results on one single page
	    /// Including the trash page
	    /// </summary>
        private const int NumberOfResultsInView = 120;

	    /// <summary>
	    /// Get the last page number
	    /// Roundup by NumberOfResultsInView
	    /// </summary>
	    /// <param name="fileIndexQueryCount">number of search results</param>
	    /// <returns>last page number (0=index)</returns>
        private static int GetLastPageNumber(int fileIndexQueryCount)
        {
            var searchLastPageNumbers = (RoundUp(fileIndexQueryCount) / NumberOfResultsInView) - 1;

            if (fileIndexQueryCount <= NumberOfResultsInView)
            {
                searchLastPageNumbers = 0;
            }
            return searchLastPageNumbers;
       }

	    /// <summary>
	    /// Roundup 
	    /// </summary>
	    /// <param name="toRound">to round e.g. 10</param>
	    /// <returns>roundup value</returns>
        public static int RoundUp(int toRound)
        {
            // 10 => ResultsInView
            if (toRound % NumberOfResultsInView == 0) return toRound;
            return (NumberOfResultsInView - toRound % NumberOfResultsInView) + toRound;
        }

	    /// <summary>
	    /// round number down
	    /// </summary>
	    /// <param name="toRound">to round</param>
	    /// <returns>round down value</returns>
        public static int RoundDown(int toRound)
        {
            return toRound - toRound % 10;
        }
    }
}
