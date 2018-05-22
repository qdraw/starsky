using System;
using System.Collections.Generic;
using System.Linq;
using starsky.Models;

namespace starsky.ViewModels
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
            if (_searchIn == null) _searchIn = new List<string>(); 
            if (FileIndexItems == null) FileIndexItems = new List<FileIndexItem>(); 
        }

        public IEnumerable<FileIndexItem> FileIndexItems { get; set; }
        public List<string> Breadcrumb { get; set; }
        public string SearchQuery { get; set; }
        public int PageNumber { get; set; }
        public int LastPageNumber { get; set; }
        public int SearchCount { get; set; }

        
        // Contains an list of Database fields to search in.
        private List<string> _searchIn;
        public List<string> SearchIn
        {
            get { return _searchIn; }
            set { _searchIn = value; }
        }
        
        public string AddSearchInStringType
        {
            set
            {
                // use ctor to have an empty list
                var fileIndexPropList = new FileIndexItem().FileIndexPropList();
                var fileIndexPropListIndex = fileIndexPropList.FindIndex(x => x.Equals(value, StringComparison.OrdinalIgnoreCase)) ;
                if (fileIndexPropListIndex != -1 )
                {
                    _searchIn.Add(fileIndexPropList[fileIndexPropListIndex]);
                } 
            }
        }
        
        
        // Search for the folling value in using SearchFor inside: _searchIn
        private List<string> _searchFor;
        public List<string> SearchFor
        {
            get { return _searchFor; }
            set { _searchFor = value; }
        }

        public string AddSearchFor
        {
            set
            {
                if (_searchFor == null) _searchFor = new List<string>();
                _searchFor.Add(value);
            }
        }





        

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
