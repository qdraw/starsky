using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using starskycore.Models;

namespace starskycore.ViewModels
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
        
	    
	    public enum SearchForOptionType
	    {
		    /// <summary>
		    ///  &gt;
		    /// </summary>
		    [Display(Name = ">")]
			GreaterThen,
		    /// <summary>
		    /// &lt;
		    /// </summary>
		    [Display(Name = "<")]
			LessThen,
		    /// <summary>
		    /// =
		    /// </summary>
		    [Display(Name = "=")]
		    Equal,
		    /// <summary>
		    /// -
		    /// </summary>
		    [Display(Name = "!-")]
		    Not
	    }
        
	    /// <summary>
	    /// Private field: Search Options &gt;, &lt;,=. (greater than sign, less than sign, equal sign) to know which field use the same indexer in _searchIn or _searchFor
	    /// </summary>
        private List<SearchForOptionType> _searchForOptions;
	    
	    /// <summary>
	    /// Search Options eg &gt;, &lt;, =. (greater than sign, less than sign, equal sign)  to know which field use the same indexer in _searchIn or _searchFor
	    /// </summary>
        public List<SearchForOptionType> SearchForOptions
        {  
            get { return _searchForOptions; }
        }

	    /// <summary>
	    /// Add first char of a string to _searchForOptions list
	    /// </summary>
	    /// <param name="value">searchFor option (e.g. =, &gt;, &lt; </param>
        public void SetAddSearchForOptions(string value)
	    {
		    if (_searchForOptions == null) _searchForOptions = new List<SearchForOptionType>();

		    switch ( value.Trim()[0] )
		    {
			    case '>':
				    _searchForOptions.Add(SearchForOptionType.GreaterThen);
				    break;
			    case '<':
				    _searchForOptions.Add(SearchForOptionType.LessThen);
				    break;
			    case '-':
				    _searchForOptions.Add(SearchForOptionType.Not);
				    break;
			    case '=':
				    _searchForOptions.Add(SearchForOptionType.Equal);
				    break;
			    case ':':
				    _searchForOptions.Add(SearchForOptionType.Equal);
				    break;
			    case ';':
				    _searchForOptions.Add(SearchForOptionType.Equal);
				    break;
		    }
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

		/// <summary>
		/// Private field: Search Operator, and or OR
		/// </summary>
		private List<bool> _searchOperatorOptions;

	    /// <summary>
	    /// Add to list in model (&amp;&amp;|| operators) true=&amp;&amp; false=||
	    /// </summary>
	    /// <param name="andOrChar"></param>
	    /// <param name="relativeLocation"></param>
	    public void SetAndOrOperator(char andOrChar, int relativeLocation = 0)
		{
			if ( _searchOperatorOptions == null ) _searchOperatorOptions = new List<bool>();

			bool andOrBool = andOrChar == '&';

			if ( char.IsWhiteSpace(andOrChar) )
			{
				andOrBool = false;
			}

			if (_searchOperatorOptions.Count == 0 && andOrChar == '|')
			{
				_searchOperatorOptions.Add(false);
			}
			
			// Store item on a different location in the List<T>
			if ( relativeLocation == 0 )
			{
				_searchOperatorOptions.Add(andOrBool);
			}
			else if ( _searchOperatorOptions.Count+relativeLocation <= -1 )
			{
				_searchOperatorOptions.Insert(0, andOrBool);
			}
			else
			{
				_searchOperatorOptions.Insert(_searchOperatorOptions.Count+relativeLocation,andOrBool);
			}
			
		}
		
		/// <summary>
		/// Search Operator, eg. || &amp;&amp;
		/// </summary>
		public List<bool> SearchOperatorOptions
		{  
			get
			{
				return _searchOperatorOptions ?? new List<bool>();
			}
		}

	    // false = (skip( continue to next item))
		public bool SearchOperatorContinue(int indexer, int max)
		{
			if ( _searchOperatorOptions == null ) return true;
			if ( indexer <= -1 || indexer > max) return true;
			// for -Datetime=1 (03-03-2019 00:00:00-03-03-2019 23:59:59), this are two queries >= fail!!
			if (indexer >= _searchOperatorOptions.Count  ) return true; // used when general words without update 
			var returnResult = _searchOperatorOptions[indexer];
			return returnResult;
		}
	    
	    /// <summary>
	    /// ||[OR] = |, else = &amp;, default = string.Emphy 
	    /// </summary>
	    /// <param name="item">searchquery</param>
	    /// <returns>bool</returns>
	    public char AndOrRegex(string item)
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
	    /// For reparsing keywords to -Tags:"keyword"
	    /// handle keywords without for example -Tags, or -DateTime prefix
	    /// </summary>
	    /// <param name="defaultQuery"></param>
	    /// <returns></returns>
	    public string ParseDefaultOption(string defaultQuery)
	    {
		    var returnQueryBuilder = new StringBuilder();

		    // Get Quoted values
		    // (["'])(\\?.)*?\1
		    
		    // Quoted or words
		    // [\w!]+|(["'])(\\?.)*?\1
		    
		    Regex inurlRegex = new Regex("[\\w!]+|([\"\'])(\\\\?.)*?\\1",
			    RegexOptions.IgnoreCase);

		    // Escape special quotes
		    defaultQuery = Regex.Replace(defaultQuery, "[“”‘’]", "\"");
		    
		    var regexInUrlMatches = inurlRegex.Matches(defaultQuery);

		    foreach ( Match regexInUrl in regexInUrlMatches )
		    {
			    if ( string.IsNullOrEmpty(regexInUrl.Value) ) continue;

			    var startIndexer = regexInUrl.Index;
			    var startLength = regexInUrl.Length;
			    var lastChar = defaultQuery[startIndexer + regexInUrl.Length -1 ];
			    
			    if ( defaultQuery[regexInUrl.Index] == '"' &&
			         lastChar == '"' ||
			         defaultQuery[regexInUrl.Index] == '\'' &&
			         lastChar == '\'' )
			    {
				    startIndexer = regexInUrl.Index + 1;
				    startLength = regexInUrl.Length - 2;
			    }
			    
			    // Get Value 
			    var searchForQuery = defaultQuery.Substring(startIndexer, startLength);
				
			    returnQueryBuilder.Append($"-Tags:\"{searchForQuery}\" ");
			    
			    SetAddSearchFor(searchForQuery);
			    SetAddSearchInStringType("tags");
			    
				// Detecting Not Queries
			    if ( ( regexInUrl.Index - 1 >= 0 && defaultQuery.ToCharArray()[regexInUrl.Index - 1] == '-' ) 
			         || ( defaultQuery.ToCharArray()[regexInUrl.Index + 2] == '-' ) )
				{
					SetAddSearchForOptions("-");
					continue;
				}
			    
			    SetAddSearchForOptions("=");
			    
		    }
			   
			//	// &&|\|\|
		    Regex andOrRegex = new Regex("&&|\\|\\|",
			    RegexOptions.IgnoreCase);
		    
		    var andOrRegexMatches = andOrRegex.Matches(defaultQuery);

		    foreach ( Match andOrValue in andOrRegexMatches )
		    {
			    SetAndOrOperator(AndOrRegex(andOrValue.Value));
		    }

		    // add for default situatons
		    if ( SearchFor.Count != SearchOperatorOptions.Count )
		    {
			    for ( int i = SearchOperatorOptions.Count; i < SearchFor.Count; i++ )
			    {
				    SetAndOrOperator(AndOrRegex("&&"));
			    }
		    }
		    
	    
		    return returnQueryBuilder.ToString();
	    }
	    
	    
	    
    }
}
