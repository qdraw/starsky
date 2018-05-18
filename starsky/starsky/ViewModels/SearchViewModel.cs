using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.ViewModels
{
    public class SearchViewModel
    {
        public IEnumerable<FileIndexItem> FileIndexItems { get; set; }
        public List<string> Breadcrumb { get; set; }
        public string SearchQuery { get; set; }
        public int PageNumber { get; set; }
        public int LastPageNumber { get; set; }
        public int SearchCount { get; set; }

        private List<string> _searchIn;
        public List<string> SearchIn => _searchIn;

//        public string SetSearchIn
//        {
//            set { return new FileIndexItem().GetType().GetProperties().ToList(); }
//        };

        

        private double _elapsedSeconds;
        public PageViewType.PageType PageType => PageViewType.PageType.Search;

        public double ElapsedSeconds
        {
            get { return _elapsedSeconds; }
            set
            {
                _elapsedSeconds = value - value % 0.001;
            }
        }
    }
}
