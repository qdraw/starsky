using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
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

        public SearchViewModel Search(string query = "", int pageNumber = 0, bool enableCache = true)
        {
            if(enableCache == false || 
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
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        private SearchViewModel SkipSearchItems(object objectSearchModel, int pageNumber)
        {
            var searchModel = objectSearchModel as SearchViewModel;            
            
            // Clone the item to avoid removeing items from cache
            searchModel = searchModel.Clone();
            
            searchModel.PageNumber = pageNumber;
            searchModel.FileIndexItems = 
                searchModel.FileIndexItems.Skip( pageNumber * NumberOfResultsInView )
                .SkipLast(searchModel.SearchCount - (pageNumber * NumberOfResultsInView ) - NumberOfResultsInView )
                    .ToHashSet().ToList();
           
            return searchModel;
        }

        /// <summary>
        /// Return all results
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageNumber"></param>
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

            WideSearch(_context.FileIndex.AsNoTracking(),model);
            NarrowSearch(model);

            // Remove duplicates from list
            model.FileIndexItems = model.FileIndexItems.GroupBy(s => s.FilePath)
                .Select(grp => grp.FirstOrDefault())
                .OrderBy(s => s.FilePath)
                .ToList();
            
            model.SearchCount = model.FileIndexItems.Count();

            model.FileIndexItems = model.FileIndexItems
                .OrderByDescending(p => p.DateTime).ToList();

            model.LastPageNumber = GetLastPageNumber(model.SearchCount);

            model.ElapsedSeconds = stopWatch.Elapsed.TotalSeconds;
            return model;
        }

        private void WideSearch(IQueryable<FileIndexItem> sourceList, SearchViewModel model)
        {
            // .AsNoTracking() => never change data to update
            for (var i = 0; i < model.SearchIn.Count; i++)
            {
                var  searchInType = (SearchViewModel.SearchInTypes)
                    Enum.Parse(typeof(SearchViewModel.SearchInTypes), model.SearchIn[i].ToLower());

                model.SearchFor[i] = model.SearchFor[i].ToLower();

                switch (searchInType)
                {
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
        }

        private void NarrowSearch(SearchViewModel model)
        {
            // Narrow Search
            for (var i = 0; i < model.SearchIn.Count; i++)
            {
                var searchInType = (SearchViewModel.SearchInTypes)
                    Enum.Parse(typeof(SearchViewModel.SearchInTypes), model.SearchIn[i].ToLower());

                switch (searchInType)
                {
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
        }

        private List<string> Split(string input)
        {
            return input.ToLower().Split(" ").ToList();
        }

        private DateTime parseDateTime(string input)
        {
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

            model.SearchQuery = "-Tags:" + "\"" + _defaultQuery.Trim()+ "\""; // escape values for !delete! query
            SearchItemName(model, "Tags");
            model.SearchQuery = _orginalSearchQuery;
            return model;
        }

        private void SearchItemName(SearchViewModel model, string itemName)
        {
            // Without double escapes:
            // (:|=|;|>|<)(([\w\!\~\-_\.\/:]+)|(\"|').+(\"|'))
            Regex inurlRegex = new Regex(
                "-" + itemName +
                "(:|=|;|>|<)(([\\w\\!\\~\\-_\\.\\/:]+)|(\"|').+(\"|'))",
                RegexOptions.IgnoreCase);
            _defaultQuery = inurlRegex.Replace(_defaultQuery,"");

            var regexInUrlMatches = inurlRegex.Matches(model.SearchQuery);
            if(!regexInUrlMatches.Any()) return;

            foreach (Match regexInUrl in regexInUrlMatches)
            {
                var item = regexInUrl.Value;
                Regex rgx = new Regex("-"+ itemName +"(:|=|;|>|<)", RegexOptions.IgnoreCase);

                // To Search Type
                var itemNameSearch = rgx.Match(item).Value;
                if (!itemNameSearch.Any()) return;

                // replace
                item = rgx.Replace(item, string.Empty);
                

                var searchForOption = itemNameSearch[itemNameSearch.Length - 1].ToString();
                model.SetAddSearchForOptions(searchForOption);                    
                
                // Remove parenthesis
                item = item.Replace("\"", string.Empty);
                item = item.Replace("'", string.Empty);
                model.SetAddSearchFor(item.Trim());
                model.SetAddSearchInStringType(itemName);
            }
                
        }

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

        // The amount of search results on one single page
        // Including the trash page
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
