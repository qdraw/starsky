using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
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
        
        public SearchViewModel Search(string query = "", int p = 0)
        {
            // Create an view model
            var model = new SearchViewModel
            {
                PageNumber = p,
                SearchQuery = query,
                Breadcrumb = new List<string> {"/",query}
            };
            
            query = query.Replace("?", "\\?");
                
            var queryRegex = new Regex(query.Replace(" ","|"), RegexOptions.IgnoreCase);
            

            // Always search case insensitive
//            model.SearchCount = _context.FileIndex.Count(i => i.Tags.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
            model.SearchCount = _context.FileIndex.Count(i => queryRegex.IsMatch(i.Tags));
            
            
            return model;
        }


        // The search feature on the website
        
        // The amount of search results on one single page
        // Including the trash page
        private const int NumberOfResultsInView = 20;

        // Query to count the number of results
        public int SearchCount(string tag = "")
        {
            var searchLastPageNumber = SearchLastPageNumber(tag);
            var searchCount = 0;
            if (searchLastPageNumber >= NumberOfResultsInView)
            {
                searchCount = _roundUp(searchLastPageNumber * NumberOfResultsInView);
            }
            else
            {
                searchCount = _searchInDatabase(tag).Count;
            }

            return  searchCount;
        }

        private static string Trim(string input)
        {
            input = input.ToLower();
            input = input.Trim();
            return input;
        }

        // Return the search results
        public IEnumerable<FileIndexItem> SearchObjectItem(string tag = "", int pageNumber = 0)
        {
            tag = Trim(tag);

            if (pageNumber < 0)
            {
                pageNumber = pageNumber * -1;
            }

            var fileIndexQueryResults = _searchInDatabase(tag)
                .OrderByDescending(p => p.DateTime).ToList();
            
            var startIndex = (pageNumber * NumberOfResultsInView);
            var searchObjectItems = new List<FileIndexItem>();

            var endIndex = _endIndex(startIndex, fileIndexQueryResults.Count);
            for (var i = startIndex; i < endIndex; i++)
            {
                searchObjectItems.Add(fileIndexQueryResults[i]);
            }


            return searchObjectItems;
        }

        private int _endIndex(int startIndex, int countOfResults)
        {
            var endIndex = startIndex + NumberOfResultsInView;
            if (endIndex >= countOfResults)
            {
                endIndex = countOfResults;
            }
            return endIndex;
        }

        private List<FileIndexItem> _searchInDatabase(string searchQuery)
        {

            // Use it for example like this: http://localhost:5000/Search?t=-inurl%3A201801&p=0
            // or "-inurl:2018 dion"
            var inurlQueryResults = new List<FileIndexItem>();
            if (searchQuery.Contains("-inurl"))
            {
                Regex inurlRegex = new Regex("-inurl(:|=|;)((\\w+|-|_|/)+|\".+\")", RegexOptions.IgnoreCase);
                // Without escaping: -inurl(:|=|;)((\w+|-|_|\/)+|".+")
                if (inurlRegex.Match(searchQuery).Success)
                {
                    var removeInUrlRegex = new Regex("-inurl(:|=|;)");
                    var searchForFileName = removeInUrlRegex.Replace(inurlRegex.Match(searchQuery).Value, "");
                    
                    // if the value is quoted
                    searchForFileName = searchForFileName.Replace("\"", "");

                    Console.WriteLine("searchForFileName");
                    Console.WriteLine(searchForFileName);
                    inurlQueryResults = _context.FileIndex.Where
                            (p => !p.IsDirectory && p.FilePath.Contains(searchForFileName))
                        .ToList();
                    
                    // Remove -inurl search query item from search 
                    searchQuery = searchQuery.Replace(inurlRegex.Match(searchQuery + " ").Value,"");
                    
                    searchQuery = Trim(searchQuery);
                    // Search inside the other keywords
                    return inurlQueryResults.Where
                            (p => !p.IsDirectory && p.Tags.Contains(searchQuery))
                        .ToList();
                }
            }

            // General Search (match Tags field)
            var tagQueryResults = _context.FileIndex.Where
                    (p => !p.IsDirectory && p.Tags.Contains(searchQuery))
                .ToList();
            
            // Order by and concating the lists
            var fileIndexQueryResults = tagQueryResults.Concat(inurlQueryResults).ToList();

            return fileIndexQueryResults;

        }
        
        // Calc. the last page
        public int SearchLastPageNumber(string tag)
        {
            tag = tag.ToLower();
            tag = tag.Trim();

            var fileIndexQueryCount = _searchInDatabase(tag).Count;

            var searchLastPageNumbers = 
                (_roundUp(fileIndexQueryCount) / NumberOfResultsInView) - 1;


            if (fileIndexQueryCount <= NumberOfResultsInView)
            {
                searchLastPageNumbers = 0;
            }

            Console.WriteLine("searchLastPageNumbers.Count");
            Console.WriteLine(searchLastPageNumbers);

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
