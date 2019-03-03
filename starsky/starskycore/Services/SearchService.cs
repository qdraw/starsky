using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.ViewModels;
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

	        //if ( skipLastNumber <= 0 ) skipLastNumber = skipLastNumber * -1;
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
	        
            model = NarrowSearch(model);

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

        private SearchViewModel WideSearch(IQueryable<FileIndexItem> sourceList, SearchViewModel model)
        {
            // .AsNoTracking() => never change data to update
            for (var i = 0; i < model.SearchIn.Count; i++)
            {
                var  searchInType = (SearchViewModel.SearchInTypes)
                    Enum.Parse(typeof(SearchViewModel.SearchInTypes), model.SearchIn[i].ToLower());

                model.SearchFor[i] = model.SearchFor[i].ToLower();

                switch (searchInType)
                {
					case SearchViewModel.SearchInTypes.imageformat:
						var  castImageFormat = (ExtensionRolesHelper.ImageFormat)
						Enum.Parse(typeof(ExtensionRolesHelper.ImageFormat), model.SearchFor[i].ToLowerInvariant());
						
						model.FileIndexItems.AddRange(sourceList.Where(
							p => p.ImageFormat == castImageFormat
						));
						
						// if you add a new type for
						// example an enum > please check: FileIndexItem.FileIndexPropList()
					break;
					case SearchViewModel.SearchInTypes.description:
						model.FileIndexItems.AddRange(sourceList.Where(
							p => p.Description.ToLower().Contains(model.SearchFor[i])
						));
					break;
					case SearchViewModel.SearchInTypes.filename:
						model.FileIndexItems.AddRange(sourceList.Where(
							p => p.FileName.ToLower().Contains(model.SearchFor[i])
						));
					break;
					case SearchViewModel.SearchInTypes.filepath:
						model.FileIndexItems.AddRange(sourceList.Where(
							p => p.FilePath.ToLower().Contains(model.SearchFor[i])
						));
					break;
					case SearchViewModel.SearchInTypes.parentdirectory:
						model.FileIndexItems.AddRange(sourceList.Where(
							p => p.ParentDirectory.ToLower().Contains(model.SearchFor[i])
						));
					break;
					case SearchViewModel.SearchInTypes.title:
						model.FileIndexItems.AddRange(sourceList.Where(
							p => p.Title.ToLower().Contains(model.SearchFor[i])
						));
					break;
					case SearchViewModel.SearchInTypes.filehash:
						model.FileIndexItems.AddRange(sourceList.Where(
							p => p.FileHash.ToLower().Contains(model.SearchFor[i])
						));
					break;
					case SearchViewModel.SearchInTypes.isdirectory:
					bool.TryParse(model.SearchFor[i].ToLowerInvariant(), out var boolIsDirectory);
						model.FileIndexItems.AddRange(sourceList.Where(
							p => p.IsDirectory == boolIsDirectory
						));
						model.SearchFor[i] = boolIsDirectory.ToString();
					break; 
					case SearchViewModel.SearchInTypes.addtodatabase:
					
						var addtodatabase = parseDateTime(model.SearchFor[i]);
						model.SearchFor[i] = addtodatabase.ToString("dd-MM-yyyy HH:mm:ss",CultureInfo.InvariantCulture);
						
						switch (model.SearchForOptions[i])
						{
							case "<":
							model.FileIndexItems.AddRange(sourceList.Where(
								p => p.AddToDatabase <= addtodatabase
							));
							break;
							case ">":
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
					
						var dateTime = parseDateTime(model.SearchFor[i]);
						model.SearchFor[i] = dateTime.ToString("dd-MM-yyyy HH:mm:ss",CultureInfo.InvariantCulture);
						
						
						// Searching for entire day
						if ( model.SearchForOptions[i] == "=" && dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0 &&
						dateTime.Millisecond == 0 )
						{
							
							model.SearchForOptions[i] = ">";
							model.SearchForOptions.Add("<");
							
							var add24Hours = dateTime.AddHours(23)
							.AddMinutes(59).AddSeconds(59).ToString(CultureInfo.InvariantCulture);
							model.SearchFor.Add(add24Hours);		                    
							model.SearchIn.Add("DateTime");
						}
						
						// faster search for searching within
						// how ever this is still triggered multiple times
						var beforeIndexSearchForOptions =
						model.SearchForOptions.IndexOf(">");
						var afterIndexSearchForOptions =
						model.SearchForOptions.IndexOf("<");
						if ( beforeIndexSearchForOptions >= 0  &&
						afterIndexSearchForOptions >= 0 ) 
						{
							var beforeDateTime = parseDateTime(model.SearchFor[beforeIndexSearchForOptions]);
							var afterDateTime = parseDateTime(model.SearchFor[afterIndexSearchForOptions]);
							
							model.FileIndexItems.AddRange(sourceList.Where(
							p => p.DateTime >= beforeDateTime && p.DateTime <= afterDateTime 
							));
							
							// We have now an extra query, and this is always AND  
							model.SetAndOrOperator('&',-2);
							
							continue;
						}
					
					
						switch (model.SearchForOptions[i])
						{
							case "<":
								model.FileIndexItems.AddRange(sourceList.Where(
									p => p.DateTime <= dateTime
								));
							break;
							case ">":
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
						
						break;
						default:
							var splitSearchFor = Split(model.SearchFor[i]);
							
							foreach (var itemSearchFor in splitSearchFor)
							{
							model.FileIndexItems.AddRange(sourceList.Where(
								p => p.Tags.ToLower().Contains(itemSearchFor)
							));
						}
					break;
                }
            }

	        return model;
        }	    

        private SearchViewModel NarrowSearch(SearchViewModel model)
        {
	        
//	        var narrowModel = new List<List<FileIndexItem>>();
	        
			// Narrow Search
			for (var i = 0; i < model.SearchIn.Count; i++)
			{
				var searchInType = (SearchViewModel.SearchInTypes)
				Enum.Parse(typeof(SearchViewModel.SearchInTypes), model.SearchIn[i].ToLower());
				
				
				// OR searches
				// var useOr = model.SearchOperatorContinue(i, model.SearchIn.Count);

				// skip OR searches
				if ( !model.SearchOperatorContinue(i, model.SearchIn.Count) )
				{
					continue;
				};
				
				
				switch (searchInType)
				{
					case SearchViewModel.SearchInTypes.imageformat:
						var  castImageFormat = (ExtensionRolesHelper.ImageFormat)
						Enum.Parse(typeof(ExtensionRolesHelper.ImageFormat), model.SearchFor[i].ToLowerInvariant());
						
						model.FileIndexItems = model.FileIndexItems.Where(
							p => p.ImageFormat == castImageFormat
						).ToList();
					break;
					
					case SearchViewModel.SearchInTypes.description:
						model.FileIndexItems = model.FileIndexItems.Where(
							p => p.Description.ToLower().Contains(model.SearchFor[i].ToLower())
						).ToList();
					break;
					
					case SearchViewModel.SearchInTypes.filename:
						model.FileIndexItems = model.FileIndexItems.Where(
							p => p.FileName.ToLower().Contains(model.SearchFor[i].ToLower())
						).ToList();
					break;
					
					case SearchViewModel.SearchInTypes.filepath:
						model.FileIndexItems = model.FileIndexItems.Where(
							p => p.FilePath.ToLower().Contains(model.SearchFor[i].ToLower())
						).ToList();
					break;
					
					case SearchViewModel.SearchInTypes.filehash:
						model.FileIndexItems = model.FileIndexItems.Where(
							p => p.FileHash.ToLower().Contains(model.SearchFor[i].ToLower())
						).ToList();
					break;
					
					case SearchViewModel.SearchInTypes.isdirectory:
						bool.TryParse(model.SearchFor[i], out var boolIsDirectory);
							model.FileIndexItems = model.FileIndexItems.Where(
								p => p.IsDirectory == boolIsDirectory
							).ToList();
					break; 
					
					case SearchViewModel.SearchInTypes.parentdirectory:
						model.FileIndexItems = model.FileIndexItems.Where(
							p => p.ParentDirectory.ToLower().Contains(model.SearchFor[i].ToLower())
						).ToList();
					break;
					
					case SearchViewModel.SearchInTypes.tags:
						// Tags are searched by multiple words
						
						var splitSearchFor = Split(model.SearchFor[i]);
						foreach (var itemSearchFor in splitSearchFor)
						{
							model.FileIndexItems = model.FileIndexItems.Where(
								p => p.Tags.ToLower().Contains(itemSearchFor)
							).ToList();
						}
					break;
					
					case SearchViewModel.SearchInTypes.title:
						model.FileIndexItems = model.FileIndexItems.Where(
						p => p.Title.ToLower().Contains(model.SearchFor[i].ToLower())
						).ToList();
					break;
					
					case SearchViewModel.SearchInTypes.addtodatabase:
					
						var addtodatabase = parseDateTime(model.SearchFor[i]);
						model.SearchFor[i] = addtodatabase.ToString("dd-MM-yyyy HH:mm:ss",CultureInfo.InvariantCulture);
						
						switch (model.SearchForOptions[i])
						{
							case "<":
								model.FileIndexItems = model.FileIndexItems.Where(
								p => p.AddToDatabase <= addtodatabase
								).ToList();
							break;
							case ">":
								model.FileIndexItems = model.FileIndexItems.Where(
								p => p.AddToDatabase >= addtodatabase
								).ToList();
							break;
							default:
								model.FileIndexItems = model.FileIndexItems.Where(
								p => p.AddToDatabase == addtodatabase
								).ToList();
							break;
						}
					
					break;
					
					case SearchViewModel.SearchInTypes.datetime:
					
						var dateTime = parseDateTime(model.SearchFor[i]);
						model.SearchFor[i] = dateTime.ToString("dd-MM-yyyy HH:mm:ss",CultureInfo.InvariantCulture);
						
						switch (model.SearchForOptions[i])
						{
							case "<":
								model.FileIndexItems = model.FileIndexItems.Where(
								p => p.DateTime <= dateTime
								).ToList();
							break;
							case ">":
								model.FileIndexItems = model.FileIndexItems.Where(
								p => p.DateTime >= dateTime
								).ToList();
							break;
							default:
								model.FileIndexItems = model.FileIndexItems.Where(
								p => p.DateTime == dateTime
								).ToList();
							break;
						}
					
					break;
				}
			}

	        return model;
        }

        private List<string> Split(string input)
        {
            return input.ToLower().Split(" ".ToCharArray()).ToList();
        }

        private DateTime parseDateTime(string input)
        {

	        // For relative values
	        if ( Regex.IsMatch(input, @"^\d+$") )
	        {
				int.TryParse(input, out var relativeValue);
				if(relativeValue >= 1) relativeValue = relativeValue * -1; // always in the past
				return DateTime.Today.AddDays(relativeValue);
	        }
	        
            var patternLab = new List<string>
            {
                "yyyy-MM-dd\\tHH:mm:ss", // < lowercase :)
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd-HH:mm:ss",
                "yyyy-MM-dd", 
                "dd-MM-yyyy", 
                "dd-MM-yyyy HH:mm:ss",
                "dd-MM-yyyy\\tHH:mm:ss",
                "MM/dd/yyyy HH:mm:ss", // < used by the next string rule 01/30/2018 00:00:00
            };
	        
            DateTime dateTime = DateTime.MinValue;
            
	        foreach (var pattern in patternLab)
            {
                DateTime.TryParseExact(input, 
                    pattern, 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out dateTime);
                if(dateTime.Year > 2) return dateTime;
            }
            return dateTime.Year > 2 ? dateTime : DateTime.Now;
        }


        private string _defaultQuery = string.Empty;
        private string _orginalSearchQuery = string.Empty;

        public SearchViewModel MatchSearch(SearchViewModel model)
        {
            _defaultQuery = model.SearchQuery;

            foreach (var itemName in new FileIndexItem().FileIndexPropList())
            {
                SearchItemName(model, itemName);
            }
            model.SearchQuery = "-Tags:" + $"\"{_defaultQuery.Trim()}\""; // changed: remove quotes, escape values for !delete! query
	        
	        SearchItemName(model, "Tags");
            model.SearchQuery = _orginalSearchQuery;
            return model;
        }

        private void SearchItemName(SearchViewModel model, string itemName)
        {
	        // ignore double quotes
	        model.SearchQuery = model.SearchQuery.Replace("\"\"", "\"");

            // Without double escapes:
            // (:|=|;|>|<)(([\w\!\~\-_\.\/:]+)|(\"|').+(\"|'))
	        // new: unescaped
	        // (:|=|;|>|<)((["'])(\\?.)*?\3|[\w\!\~\-_\.\/:]+)( \|\|| \&\&)?
            Regex inurlRegex = new Regex(
                "-" + itemName +
                "(:|=|;|>|<)(([\"\'])(\\\\?.)*?\\3|[\\w\\!\\~\\-_\\.\\/:]+)( \\|\\|| \\&\\&)?",
                RegexOptions.IgnoreCase);
            _defaultQuery = inurlRegex.Replace(_defaultQuery,"");

            var regexInUrlMatches = inurlRegex.Matches(model.SearchQuery);
            if(regexInUrlMatches.Count == 0) return;

            foreach (Match regexInUrl in regexInUrlMatches)
            {
                var itemQuery = regexInUrl.Value;
	            
	            // ignore fake results
	            if ( string.IsNullOrEmpty(itemQuery) ) continue;

	            // put ||&& in operator field => next regex > removed
	            model.SetAndOrOperator(AndOrRegex(itemQuery));
	            
	            Regex rgx = new Regex("-"+ itemName +"(:|=|;|>|<)", RegexOptions.IgnoreCase);

                // To Search Type
                var itemNameSearch = rgx.Match(itemQuery).Value;
                if (!itemNameSearch.Any()) continue;

                // replace
                itemQuery = rgx.Replace(itemQuery, string.Empty);

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
	    /// ||[OR] = |, else = &amp;, default = string.Emphy 
	    /// </summary>
	    /// <param name="item">searchquery</param>
	    /// <returns>bool</returns>
	    private char AndOrRegex(string item)
	    {
		    // (\|\||\&\&)$
		    Regex rgx = new Regex(@"(\|\||\&\&)$", RegexOptions.IgnoreCase);

		    // To Search Type
		    var lastStringValue = rgx.Match(item).Value;
		    
		    // set default
		    if ( string.IsNullOrEmpty(lastStringValue) ) lastStringValue = string.Empty;


		    if ( lastStringValue == "||" ) return '|';
		    return '&';
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

        public string QueryShortcuts(string query)
        {
            query = query.Replace("-inurl", "-FilePath");
            return query;
        }

	    /// <summary>
	    /// The amount of search results on one single page
	    /// Including the trash page
	    /// </summary>
        private const int NumberOfResultsInView = 20;

        private int GetLastPageNumber(int fileIndexQueryCount)
        {
            var searchLastPageNumbers = (RoundUp(fileIndexQueryCount) / NumberOfResultsInView) - 1;

            if (fileIndexQueryCount <= NumberOfResultsInView)
            {
                searchLastPageNumbers = 0;
            }
            return searchLastPageNumbers;
       }

        // Round features:
        public int RoundUp(int toRound)
        {
            // 10 => ResultsInView
            if (toRound % NumberOfResultsInView == 0) return toRound;
            return (NumberOfResultsInView - toRound % NumberOfResultsInView) + toRound;
        }

        public int RoundDown(int toRound)
        {
            return toRound - toRound % 10;
        }

    }
}
