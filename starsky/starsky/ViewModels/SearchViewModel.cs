using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private double _elapsedSeconds;

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
