using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.ViewModels;

namespace starskycore.Services
{
    public class SearchService : ISearch
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly AppSettings _appSettings;

        public SearchService(
            ApplicationDbContext context, 
            IMemoryCache memoryCache = null,
            AppSettings appSettings = null)
        {
            _context = context;
            _cache = memoryCache;
            _appSettings = appSettings;
        }

	    /// <summary>
	    /// Search in database
	    /// </summary>
	    /// <param name="query">where to search in</param>
	    /// <param name="pageNumber">current page (0 = page 1)</param>
	    /// <param name="enableCache">enable searchcache (in trash this is disabled) </param>
	    /// <returns></returns>
        public SearchViewModel Search(string query = "", int pageNumber = 0, bool enableCache = true)
	    {
		    if ( !string.IsNullOrEmpty(query) && query.Length >= 500 )
		    {
			    throw new ArgumentException("Search Input Query is longer then 500 chars");
		    }

            if(!enableCache || 
               _cache == null || _appSettings?.AddMemoryCache == false) 
                return SkipSearchItems(SearchDirect(query),pageNumber);

            // Return values from IMemoryCache
            var queryCacheName = "search-" + query;
            
            // Return Cached object if it exist
            if (_cache.TryGetValue(queryCacheName, out var objectSearchModel))
                return SkipSearchItems(objectSearchModel, pageNumber);
            
            // Try to catch a new object
            objectSearchModel = SearchDirect(query);
            _cache.Set(queryCacheName, objectSearchModel, new TimeSpan(0,10,0));
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
        private SearchViewModel SkipSearchItems(object objectSearchModel, int pageNumber)
        {
            var searchModel = objectSearchModel as SearchViewModel;            
            
            // Clone the item to avoid removeing items from cache
            searchModel = searchModel.Clone();
            
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
        private SearchViewModel SearchDirect(string query = "")
        {

            var stopWatch = Stopwatch.StartNew();

            // Create an view model
            var model = new SearchViewModel
            {
                SearchQuery = query ?? string.Empty,
                Breadcrumb = new List<string> {"/", query ?? string.Empty  }
                // Null check will safe you from error 500 with Empty request
            };

            if (query == null) return model;

            _orginalSearchQuery = model.SearchQuery;

            model.SearchQuery = QuerySafe(model.SearchQuery);
            model.SearchQuery = QueryShortcuts(model.SearchQuery);
            model = MatchSearch(model);

            model = WideSearch(_context.FileIndex.AsNoTracking(),model);
	        
            model = model.NarrowSearch(model);

            // Remove duplicates from list
            model.FileIndexItems = model.FileIndexItems.GroupBy(s => s.FilePath)
                .Select(grp => grp.FirstOrDefault())
                .OrderBy(s => s.FilePath)
                .ToList();
            
            model.SearchCount = model.FileIndexItems.Count;

            model.FileIndexItems = model.FileIndexItems
                .OrderByDescending(p => p.DateTime).ToList();

            model.LastPageNumber = GetLastPageNumber(model.SearchCount);

            model.ElapsedSeconds = stopWatch.Elapsed.TotalSeconds;
            return model;
        }

	    /// <summary>
	    /// Do a query with multiple tags, then add it to the model
	    /// </summary>
	    /// <param name="sourceList">where to query in</param>
	    /// <param name="model">where to add</param>
	    /// <returns>model of type SearchViewModel</returns>
	    private SearchViewModel WideSearchTagsFast(IQueryable<FileIndexItem> sourceList,
		    SearchViewModel model)
	    {
		    var tagsKeywords = new List<string>();
		    for ( int i = 0; i < model.SearchIn.Count; i++ )
		    {
			    if ( model.SearchIn[i].ToLowerInvariant() == nameof(SearchViewModel.SearchInTypes.tags) )
			    {
				    tagsKeywords.Add(model.SearchFor[i].ToLowerInvariant());
			    }
		    }
		    
		    var predicate = PredicateBuilder.False<FileIndexItem>();

		    foreach (string keyword in tagsKeywords)
		    {
			    // Not ToLowerInvariant() due the fact that this is not supported in EF SQL
			    predicate = predicate.Or (p => p.Tags.ToLower().Contains (keyword));
		    }
		    model.FileIndexItems.AddRange(sourceList.Where(predicate));
		    return model;
	    }

	    /// <summary>
	    /// Main method to query the database, in other function there is sorting needed
	    /// </summary>
	    /// <param name="sourceList">IQueryable database</param>
	    /// <param name="model">temp output model</param>
	    /// <returns>search model with content</returns>
	    private SearchViewModel WideSearch(IQueryable<FileIndexItem> sourceList,
		    SearchViewModel model)
	    {

		    // Search for tags in 1 query
		    model = WideSearchTagsFast(sourceList, model);
		    
		    // .AsNoTracking() => never change data to update
		    for ( var i = 0; i < model.SearchIn.Count; i++ )
		    {
			    var searchInType = ( SearchViewModel.SearchInTypes )
				    Enum.Parse(typeof(SearchViewModel.SearchInTypes), model.SearchIn[i].ToLowerInvariant());

			    model.SearchFor[i] = model.SearchFor[i].ToLowerInvariant();
			    
			    switch ( searchInType )
			    {
				    case SearchViewModel.SearchInTypes.imageformat:

					    Enum.TryParse<ExtensionRolesHelper.ImageFormat>(
						    model.SearchFor[i].ToLowerInvariant(), out var castImageFormat);
				    
					    // todo fix issue:
					    model.FileIndexItems.Add(sourceList.FirstOrDefault(p => p.ImageFormat == castImageFormat));
					    
					    // model.FileIndexItems.AddRange(sourceList.Where(
						   //  p => p.ImageFormat != ExtensionRolesHelper.ImageFormat.unknown && p.ImageFormat == castImageFormat
					    // ));

					    // if you add a new type for
					    // example an enum > please check: FileIndexItem.FileIndexPropList()
					    break;
				    case SearchViewModel.SearchInTypes.description:
					    model.FileIndexItems.AddRange(sourceList.Where(
						    p => p.Description.ToLowerInvariant().Contains(model.SearchFor[i])
					    ));
					    break;
				    case SearchViewModel.SearchInTypes.filename:
					    model.FileIndexItems.AddRange(sourceList.Where(
						    p => p.FileName.ToLowerInvariant().Contains(model.SearchFor[i])
					    ));
					    break;
				    case SearchViewModel.SearchInTypes.filepath:
					    // The LINQ expression 'where [p].FilePath.ToLowerInvariant().Contains(__get_Item_0)'
					    // could not be translated and will be evaluated locally.
					    model.FileIndexItems.AddRange(sourceList.Where(
						    p => p.FilePath.ToLower().Contains(model.SearchFor[i])
					    ));
					    break;
				    case SearchViewModel.SearchInTypes.parentdirectory:
					    model.FileIndexItems.AddRange(sourceList.Where(
						    p => p.ParentDirectory.ToLowerInvariant().Contains(model.SearchFor[i])
					    ));
					    break;
				    case SearchViewModel.SearchInTypes.title:
					    model.FileIndexItems.AddRange(sourceList.Where(
						    p => p.Title.ToLowerInvariant().Contains(model.SearchFor[i])
					    ));
					    break;
				    case SearchViewModel.SearchInTypes.filehash:
					    model.FileIndexItems.AddRange(sourceList.Where(
						    p =>  p.FileHash != null && p.FileHash.ToLowerInvariant() == model.SearchFor[i]
					    ));
					    break;
				    case SearchViewModel.SearchInTypes.isdirectory:
					    bool.TryParse(model.SearchFor[i].ToLowerInvariant(),
						    out var boolIsDirectory);
					    model.FileIndexItems.AddRange(sourceList.Where(
						    p => p.IsDirectory == boolIsDirectory
					    ));
					    model.SearchFor[i] = boolIsDirectory.ToString();
					    break;
				    
				    case SearchViewModel.SearchInTypes.lastedited:

					    var lastEdited = model.ParseDateTime(model.SearchFor[i]);
					    model.SearchFor[i] = lastEdited.ToString("dd-MM-yyyy HH:mm:ss",
						    CultureInfo.InvariantCulture);

					    switch ( model.SearchForOptions[i] )
					    {
						    case SearchViewModel.SearchForOptionType.LessThen:
							    model.FileIndexItems.AddRange(sourceList.Where(
								    p => p.LastEdited <= lastEdited
							    ));
							    break;
						    case SearchViewModel.SearchForOptionType.GreaterThen:
							    model.FileIndexItems.AddRange(sourceList.Where(
								    p => p.LastEdited >= lastEdited
							    ));
							    break;
						    default:
							    model.FileIndexItems.AddRange(sourceList.Where(
								    p => p.LastEdited == lastEdited
							    ));
							    break;
					    }

					    break;
				    
				    case SearchViewModel.SearchInTypes.addtodatabase:

					    var addtodatabase = model.ParseDateTime(model.SearchFor[i]);
					    model.SearchFor[i] = addtodatabase.ToString("dd-MM-yyyy HH:mm:ss",
						    CultureInfo.InvariantCulture);

					    switch ( model.SearchForOptions[i] )
					    {
						    case SearchViewModel.SearchForOptionType.LessThen:
							    model.FileIndexItems.AddRange(sourceList.Where(
								    p => p.AddToDatabase <= addtodatabase
							    ));
							    break;
						    case SearchViewModel.SearchForOptionType.GreaterThen:
							    model.FileIndexItems.AddRange(sourceList.Where(
								    p => p.AddToDatabase >= addtodatabase
							    ));
							    break;
						    default:
							    model.FileIndexItems.AddRange(sourceList.Where(
								    p => p.AddToDatabase == addtodatabase
							    ));
							    break;
					    }

					    break;

				    case SearchViewModel.SearchInTypes.datetime:
					    WideSearchDateTimeGet(sourceList,model,i);
					    break;
				    case SearchViewModel.SearchInTypes.tags:
						// don't do anything with tags here:  WideSearchTagsFast
					    break;
			    }
		    }

		    return model;
	    }
	    
	    /// <summary>
	    /// Query for DateTime: in between values, entire days, from, type of queries
	    /// </summary>
	    /// <param name="sourceList">Query Source</param>
	    /// <param name="model">output</param>
	    /// <param name="indexer">number of search query (i)</param>
	    private void WideSearchDateTimeGet(IQueryable<FileIndexItem> sourceList,
		    SearchViewModel model, int indexer)
	    {
			var dateTime = model.ParseDateTime(model.SearchFor[indexer]);
			model.SearchFor[indexer] = dateTime.ToString("dd-MM-yyyy HH:mm:ss",
				CultureInfo.InvariantCulture);


			// Searching for entire day
			if ( model.SearchForOptions[indexer] == SearchViewModel.SearchForOptionType.Equal
				 && dateTime.Hour == 0 &&
				 dateTime.Minute == 0 && dateTime.Second == 0 &&
				 dateTime.Millisecond == 0 )
			{

				model.SearchForOptions[indexer] = SearchViewModel.SearchForOptionType.GreaterThen;
				model.SearchForOptions.Add(SearchViewModel.SearchForOptionType.LessThen);

				var add24Hours = dateTime.AddHours(23)
					.AddMinutes(59).AddSeconds(59)
					.ToString(CultureInfo.InvariantCulture);
				model.SearchFor.Add(add24Hours);
				model.SearchIn.Add("DateTime");
			}

			// faster search for searching within
			// how ever this is still triggered multiple times
			var beforeIndexSearchForOptions =
				model.SearchForOptions.IndexOf(SearchViewModel.SearchForOptionType.GreaterThen);
			var afterIndexSearchForOptions =
				model.SearchForOptions.IndexOf(SearchViewModel.SearchForOptionType.LessThen);
			if ( beforeIndexSearchForOptions >= 0 &&
				 afterIndexSearchForOptions >= 0 )
			{
				var beforeDateTime =
					model.ParseDateTime(model.SearchFor[beforeIndexSearchForOptions]);
				
				var afterDateTime =
					model.ParseDateTime(model.SearchFor[afterIndexSearchForOptions]);

				model.FileIndexItems.AddRange(sourceList.Where(
					p => p.DateTime >= beforeDateTime && p.DateTime <= afterDateTime
				));

				// We have now an extra query, and this is always AND  
				model.SetAndOrOperator('&', -2);

				return;
			}

			// Normal search
			switch ( model.SearchForOptions[indexer] )
			{
				case SearchViewModel.SearchForOptionType.LessThen:
					// "<":
					model.FileIndexItems.AddRange(sourceList.Where(
						p => p.DateTime <= dateTime
					));
					break;
				case SearchViewModel.SearchForOptionType.GreaterThen:
					model.FileIndexItems.AddRange(sourceList.Where(
						p => p.DateTime >= dateTime
					));
					break;
				default:
					model.FileIndexItems.AddRange(sourceList.Where(
						p => p.DateTime == dateTime
					));
					break;
			}
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
			if ( string.IsNullOrWhiteSpace(model.SearchQuery) ) return model;

	        _defaultQuery = model.SearchQuery;

            foreach (var itemName in new FileIndexItem().FileIndexPropList())
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
	        // ignore double quotes
	        model.SearchQuery = model.SearchQuery.Replace("\"\"", "\"");
	        
	        // Escape special quotes
	        model.SearchQuery = Regex.Replace(model.SearchQuery, "[“”‘’]", "\"");

            // Without double escapes:
            // (:|=|;|>|<)(([\w\!\~\-_\.\/:]+)|(\"|').+(\"|'))
	        // new: unescaped
	        // (:|=|;|>|<|-)((["'])(\\?.)*?\3|[\w\!\~\-_\.\/:]+)( \|\|| \&\&)?
            Regex inurlRegex = new Regex(
                "-" + itemName +
                "(:|=|;|>|<|-)(([\"\'])(\\\\?.)*?\\3|[\\w\\!\\~\\-_\\.\\/:]+)( \\|\\|| \\&\\&)?",
                RegexOptions.IgnoreCase);
	        
            _defaultQuery = inurlRegex.Replace(_defaultQuery,"");
	        // the current query is removed from the list, so the next item will not search on it

            var regexInUrlMatches = inurlRegex.Matches(model.SearchQuery);
            if(regexInUrlMatches.Count == 0) return;

            foreach (Match regexInUrl in regexInUrlMatches)
            {
                var itemQuery = regexInUrl.Value;
	            
	            // ignore fake results
	            if ( string.IsNullOrEmpty(itemQuery) ) continue;

	            // put ||&& in operator field => next regex > removed
	            model.SetAndOrOperator(model.AndOrRegex(itemQuery));
	            
	            Regex rgx = new Regex("-"+ itemName +"(:|=|;|>|<|-)", RegexOptions.IgnoreCase);

                // To Search Type
                var itemNameSearch = rgx.Match(itemQuery).Value;
                if (!itemNameSearch.Any()) continue;

                // replace
                itemQuery = rgx.Replace(itemQuery, string.Empty);

	            // Option last of itemNameSearch
                var searchForOption = itemNameSearch[itemNameSearch.Length - 1].ToString();
                model.SetAddSearchForOptions(searchForOption);                    
                
                // Remove parenthesis
                itemQuery = itemQuery.Replace("\"", string.Empty);
                itemQuery = itemQuery.Replace("'", string.Empty);

	            // Remove || / && at the end of the string
	            // (\|\||\&\&)$
	            string pattern = "(\\|\\||\\&\\&)$";
				itemQuery = Regex.Replace(itemQuery, pattern, string.Empty);
	            
                model.SetAddSearchFor(itemQuery.Trim());
                model.SetAddSearchInStringType(itemName);
           
            }
                
        }



	    /// <summary>
	    /// Trim value (remove spaces)
	    /// </summary>
	    /// <param name="query">searchQuery</param>
	    /// <returns>trimmed value</returns>
        public string QuerySafe(string query)
        {
            query = query.Trim();
            return query;
        }

	    /// <summary>
	    /// Allow -inurl shortcut
	    /// </summary>
	    /// <param name="query"></param>
	    /// <returns></returns>
        public string QueryShortcuts(string query)
        {
            query = query.Replace("-inurl", "-FilePath");
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
        private int GetLastPageNumber(int fileIndexQueryCount)
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
        public int RoundUp(int toRound)
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
        public int RoundDown(int toRound)
        {
            return toRound - toRound % 10;
        }

    }
}
