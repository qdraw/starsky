using System;
using System.Collections.Generic;
using System.ComponentModel;
using starsky.Models;
using starskycore.Models;

namespace starsky.ViewModels
{
    public class SearchViewModel
    {
        public SearchViewModel()
        {
            if (_searchIn == null) _searchIn = new List<string>(); 
            if (FileIndexItems == null) FileIndexItems = new List<FileIndexItem>(); 
	        
	        _dateTime = DateTime.Now;
        }

	    /// <summary>
	    /// Private field: Used to know how old the search query is
	    /// </summary>
	    private readonly DateTime _dateTime;
	    
	    /// <summary>
	    /// Used to know how old the search query is
	    /// </summary>
	    public double Offset =>   Math.Round(Math.Abs((DateTime.Now - _dateTime).TotalSeconds),2);

	    public List<FileIndexItem> FileIndexItems { get; set; }
        public List<string> Breadcrumb { get; set; }
        public string SearchQuery { get; set; }
        public int PageNumber { get; set; }
        public int LastPageNumber { get; set; }

        public int SearchCount { get; set; }

	    /// <summary>
	    /// Types to search in e.g. -Title=Test
	    /// </summary>
        public enum SearchInTypes
        {
            [Description("value 3")]
            // https://www.codementor.io/cerkit/giving-an-enum-a-string-value-using-the-description-attribute-6b4fwdle0
            filepath = 0,
            filename = 1,
            parentdirectory = 2,
            tags = 3,
            description = 4,
            title = 5,
            datetime = 6,
            addtodatabase = 7,
	        filehash = 8,
	        isdirectory = 9,
	        imageformat = 10
        }

        
        /// <summary>
        /// Private Field: Contains an list of Database fields to search in.
        /// </summary>
        private readonly List<string> _searchIn;
	    
	    /// <summary>
	    /// Contains an list of Database fields to search in.
	    /// </summary>
        public List<string> SearchIn => _searchIn;

        public void SetAddSearchInStringType(string value)
        {
            // use ctor to have an empty list
            var fileIndexPropList = new FileIndexItem().FileIndexPropList();
            var fileIndexPropListIndex = fileIndexPropList.FindIndex
                (x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (fileIndexPropListIndex != -1 )
            {
                _searchIn.Add(fileIndexPropList[fileIndexPropListIndex]);
            } 
        }
        
        
	    /// <summary>
	    /// Private field: Search for the following value in using SearchFor inside: _searchIn
	    /// </summary>
        private List<string> _searchFor;
	    
	    /// <summary>
	    /// The values to search for, to know which field use the same indexer in _searchIn
	    /// </summary>
        public List<string> SearchFor
        {  
            // don't change it to 'SearchFor => _searchFor'
            get { return _searchFor; }
        }

	    /// <summary>
	    /// Add string to searchFor list
	    /// </summary>
	    /// <param name="value"></param>
        public void SetAddSearchFor(string value)
        {
            if (_searchFor == null) _searchFor = new List<string>();
            _searchFor.Add(value.Trim());
        }
        
        
	    /// <summary>
	    /// Private field: Search Options eg >, <, =. to know which field use the same indexer in _searchIn or _searchFor
	    /// </summary>
        private List<string> _searchForOptions;
	    
	    /// <summary>
	    /// Search Options eg >, <, =. to know which field use the same indexer in _searchIn or _searchFor
	    /// </summary>
        public List<string> SearchForOptions
        {  
            get { return _searchForOptions; }
        }

	    /// <summary>
	    /// Add first char of a string to _searchForOptions list
	    /// </summary>
	    /// <param name="value"></param>
        public void SetAddSearchForOptions(string value)
        {
            if (_searchForOptions == null) _searchForOptions = new List<string>();
            _searchForOptions.Add(value.Trim()[0].ToString());
        }

        public string PageType { get; } = PageViewType.PageType.Search.ToString();

	    /// <summary>
	    /// Private field: Know in seconds how much time a database query is.
	    /// </summary>
	    private double _elapsedSeconds;

	    /// <summary>
	    /// Know in seconds how much time a database query is. Rounded by 3 decimals
	    /// </summary>
        public double ElapsedSeconds
        {
            get { return _elapsedSeconds; }
            set
            {
                _elapsedSeconds = value - value % 0.001;
            }
        }

	    /// <summary>
	    /// Copy the current object in memory
	    /// </summary>
	    /// <returns></returns>
	    public SearchViewModel Clone()
        {
            return (SearchViewModel) MemberwiseClone();
        }
        
    }
}
