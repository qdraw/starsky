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

        private const int ResultsInView = 50;

        public IEnumerable<FileIndexItem> SearchObjectItem(string tag = "", int pageNumber = 0)
        {
            tag = tag.ToLower();

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

        public int SearchLastPageNumber(string tag)
        {
            tag = tag.ToLower();
            var fileIndexQueryCount = _context.FileIndex.Count
                (p => !p.IsDirectory && p.Tags.Contains(tag));

            var searchLastPageNumbers = (fileIndexQueryCount / ResultsInView) - 1;

            if (fileIndexQueryCount <= ResultsInView)
            {
                searchLastPageNumbers = 0;
            }

            return searchLastPageNumbers;
        }



    }
}
