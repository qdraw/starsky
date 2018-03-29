using System.Collections.Generic;
using System.Linq;
using starsky.Data;
using starsky.Interfaces;
using starsky.Models;

namespace starsky.Services
{
    public class SearchService : ISearch
    {
        private readonly ApplicationDbContext _context;

        public SearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        // The search feature on the website
        
        // The amount of search results on one single page
        // Including the trash page
        private const int ResultsInView = 40;

        // Query to count the number of results
        public int SearchCount(string tag = "")
        {
            return _context.FileIndex.Count
                (p => !p.IsDirectory && p.Tags.Contains(tag));
        }

        // Return the search results
        public IEnumerable<FileIndexItem> SearchObjectItem(string tag = "", int pageNumber = 0)
        {
            tag = tag.ToLower();
            tag = tag.Trim();

            if (pageNumber < 0)
            {
                pageNumber = pageNumber * -1;
            }

            var searchObjectItems = new List<FileIndexItem>();

            var fileIndexQueryResults = _context.FileIndex.Where
                (p => !p.IsDirectory && p.Tags.Contains(tag)).OrderByDescending(p => p.DateTime).ToList();

            var startIndex = (pageNumber * ResultsInView);

            var endIndex = startIndex + ResultsInView;
            if (endIndex >= fileIndexQueryResults.Count)
            {
                endIndex = fileIndexQueryResults.Count;
            }

            var i = startIndex;
            while (i < endIndex)
            {
                searchObjectItems.Add(fileIndexQueryResults[i]);
                i++;
            }

            return searchObjectItems;
        }

        // Calc. the last page
        public int SearchLastPageNumber(string tag)
        {
            tag = tag.ToLower();
            tag = tag.Trim();

            var fileIndexQueryCount = _context.FileIndex.Count
                (p => !p.IsDirectory && p.Tags.Contains(tag));

            var searchLastPageNumbers = 
                (_roundUp(fileIndexQueryCount) / ResultsInView) - 1;


            if (fileIndexQueryCount <= ResultsInView)
            {
                searchLastPageNumbers = 0;
            }

            return searchLastPageNumbers;
        }

        // Round features:
        private int _roundUp(int toRound)
        {
            // 10 => ResultsInView
            if (toRound % ResultsInView == 0) return toRound;
            return (ResultsInView - toRound % ResultsInView) + toRound;
        }

       /*private int _RoundDown(int toRound)
        {
            return toRound - toRound % 10;
        }*/


    }
}
