using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Services
{
    public class SearchService : ISearch
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool ContainInAllFields(SearchViewModel model, FileIndexItem item )
        {
            var isMatchList = new List<bool>();

            for (int i = 0; i < model.SearchIn.Count; i++)
            {
                var queryRegex = new Regex(model.SearchFor[i].Replace(" ", "|"), RegexOptions.IgnoreCase);
                model.SearchQuery = model.SearchQuery.Trim();
                isMatchList.Add(queryRegex.IsMatch(item.GetPropValue(model.SearchIn[i]).ToString()));
            }
            if (isMatchList.Contains(false)) return false;
            return true;
        }

        public SearchViewModel Search(string query = "", int pageNumber = 0)
        {
            var stopWatch = Stopwatch.StartNew();
            
            // Create an view model
            var model = new SearchViewModel
            {
                PageNumber = pageNumber,
                SearchQuery = query,
                Breadcrumb = new List<string> {"/",query}
            };

            _orginalSearchQuery = model.SearchQuery;

            model.SearchQuery = QuerySafe(model.SearchQuery);
            model = MatchSearch(model);
            
            // Calculate how much items we have
            model.SearchCount = 0;
            model.SearchCount += _context.FileIndex.Count(
                p => ContainInAllFields(model,p)
            );
            model.LastPageNumber = 0;

            model.ElapsedSeconds = stopWatch.Elapsed.TotalSeconds;
            if (model.SearchCount == 0) return model;

            model.FileIndexItems = _context.FileIndex.Where(p => ContainInAllFields(model, p))
                .OrderByDescending(p => p.DateTime).ToList()
                .Skip( pageNumber * NumberOfResultsInView )
                .SkipLast( model.SearchCount - (pageNumber * NumberOfResultsInView ) - NumberOfResultsInView );  

            model.LastPageNumber = GetLastPageNumber(model.SearchCount);
            
            model.ElapsedSeconds = stopWatch.Elapsed.TotalSeconds;
            return model;
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

            model.SearchQuery = "-Tags:" + _defaultQuery.Trim();
            SearchItemName(model, "Tags");
            model.SearchQuery = _orginalSearchQuery;
            return model;
        }

        private void SearchItemName(SearchViewModel model, string itemName)
        {
            Regex inurlRegex = new Regex("-"+ itemName +"(:|=|;)((\\w+|-|_|/)+|(\"|').+(\"|'))", RegexOptions.IgnoreCase);
            _defaultQuery = inurlRegex.Replace(_defaultQuery,"");
            if (inurlRegex.Match(model.SearchQuery).Success)
            {
                var item = inurlRegex.Match(model.SearchQuery).Value;
                Regex rgx = new Regex("-"+ itemName +"(:|=|;)", RegexOptions.IgnoreCase);
                item = rgx.Replace(item, string.Empty);
                item = item.Replace("\"", string.Empty);
                item = item.Replace("'", string.Empty);
                model.AddSearchFor = item;
                model.AddSearchInStringType = itemName;
            }
        }

        private string QuerySafe(string query)
        {
            query = query.Trim();
            query = query.Replace("?", "\\?");
            query = query.Replace("!", "\\!");
            return query;
        }

        // The amount of search results on one single page
        // Including the trash page
        private const int NumberOfResultsInView = 20;

        private int GetLastPageNumber(int fileIndexQueryCount)
        {
            var searchLastPageNumbers = (_roundUp(fileIndexQueryCount) / NumberOfResultsInView) - 1;

            if (fileIndexQueryCount <= NumberOfResultsInView)
            {
                searchLastPageNumbers = 0;
            }
            return searchLastPageNumbers;
       }

        // Round features:
        private int _roundUp(int toRound)
        {
            // 10 => ResultsInView
            if (toRound % NumberOfResultsInView == 0) return toRound;
            return (NumberOfResultsInView - toRound % NumberOfResultsInView) + toRound;
        }

        private int _RoundDown(int toRound)
        {
            return toRound - toRound % 10;
        }


    }
}
