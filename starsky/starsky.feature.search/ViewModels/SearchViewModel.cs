﻿#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.search.ViewModels
{
    [SuppressMessage("ReSharper", "ArrangeAccessorOwnerBody")]
    public class SearchViewModel
    {
        public SearchViewModel()
        {
	        // init default values
            _searchIn ??= new List<string>();
            FileIndexItems ??= new List<FileIndexItem>();
            Breadcrumb ??= new List<string>();
            SearchQuery ??= string.Empty;

	        // to know how long a query takes
	        _dateTime = DateTime.Now;
        }

	    /// <summary>
	    /// Private field: Used to know how old the search query is
	    /// </summary>
	    private readonly DateTime _dateTime;
	    
	    /// <summary>
	    /// Items on the page
	    /// </summary>
	    public List<FileIndexItem>? FileIndexItems { get; set; }
        
	    /// <summary>
	    /// Full location specification
	    /// </summary>
	    // ReSharper disable once UnusedAutoPropertyAccessor.Global
	    // ReSharper disable once CollectionNeverQueried.Global
	    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
	    public List<string> Breadcrumb { get; set; }
        
	    /// <summary>
	    /// Where to search for
	    /// </summary>
	    public string SearchQuery { get; set; }
	    
	    /// <summary>
	    /// Current page number (index=0)
	    /// </summary>
	    // ReSharper disable once UnusedAutoPropertyAccessor.Global
	    public int PageNumber { get; set; }
	    
	    /// <summary>
	    /// The last page (index=0)
	    /// </summary>
        public int LastPageNumber { get; set; }

	    /// <summary>
	    /// Number of search results
	    /// </summary>
        public int SearchCount { get; set; }

	    /// <summary>
	    /// Number of search results (Different name)
	    /// </summary>
	    public int CollectionsCount => SearchCount;

	    /// <summary>
	    /// Types to search in e.g. -Title=Test
	    /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
	    public enum SearchInTypes
        {
	        tags = 0,
	        filepath = 1,
            filename = 2,
            // ReSharper disable once IdentifierTypo
            parentdirectory = 3,
            description = 4,
            title = 5,
            datetime = 6,
            // ReSharper disable once IdentifierTypo
            addtodatabase = 7,
            // ReSharper disable once IdentifierTypo
            filehash = 8,
	        // ReSharper disable once IdentifierTypo
	        isdirectory = 9,
	        // ReSharper disable once IdentifierTypo
	        imageformat = 10,
	        // ReSharper disable once IdentifierTypo
	        lastedited = 11,
	        make = 12,
	        model = 13,
	        // ReSharper disable once IdentifierTypo
	        colorclass = 14,
	        software = 15
        }

        
        /// <summary>
        /// Private Field: Contains an list of Database fields to search in.
        /// </summary>
        private List<string> _searchIn;
	    
	    /// <summary>
	    /// Contains an list of Database fields to search in.
	    /// </summary>
        public List<string> SearchIn => _searchIn;

	    /// <summary>
	    /// In which database field the search query is needed
	    /// </summary>
	    /// <param name="value">Search field name e.g. Tags</param>
        public void SetAddSearchInStringType(string value)
	    {
		    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
		    _searchIn ??= new List<string>();
		    
            // use ctor to have an empty list
            var fileIndexPropList = FileIndexItem.FileIndexPropList();
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
        private List<string>? _searchFor = new();
	    
	    /// <summary>
	    /// The values to search for, to know which field use the same indexer in _searchIn
	    /// </summary>
        public List<string> SearchFor
        {  
            // don't change it to 'SearchFor => _searchFor'
            get
            {
	            return _searchFor ?? new List<string>();
            }
        }

	    /// <summary>
	    /// Add string to searchFor list
	    /// </summary>
	    /// <param name="value"></param>
	    public void SetAddSearchFor(string value)
	    {
		    _searchFor ??= new List<string>();
		    _searchFor.Add(value.Trim().ToLowerInvariant());
	    }
        
	    /// <summary>
	    /// The search for types
	    /// </summary>
	    [DataContract]
		[JsonConverter(typeof(JsonStringEnumConverter))]
	    public enum SearchForOptionType
	    {
		    /// <summary>
		    ///  &gt;
		    /// </summary>
		    [Display(Name = ">")] // in json it is GreaterThen
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
        private List<SearchForOptionType>? _searchForOptions;

	    /// <summary>
	    /// Search Options eg &gt;, &lt;, =. (greater than sign, less than sign, equal sign)  to know which field use the same indexer in _searchIn or _searchFor
	    /// </summary>
	    public List<SearchForOptionType> SearchForOptions
        {  
            get
            {
	            return _searchForOptions ?? new List<SearchForOptionType>();
            }
        }

	    /// <summary>
	    /// Add first char of a string to _searchForOptions list
	    /// </summary>
	    /// <param name="value">searchFor option (e.g. =, &gt;, &lt; </param>
        public void SetAddSearchForOptions(string value)
	    {
		    _searchForOptions ??= new List<SearchForOptionType>();

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

	    /// <summary>
	    /// The type of page returns, (Search or Trash)
	    /// </summary>
        public string PageType {
		    get
		    {
			    if (string.IsNullOrEmpty(SearchQuery) ) return PageViewType.PageType.Search.ToString();
			    return SearchQuery == "!delete!" ? PageViewType.PageType.Trash.ToString() : PageViewType.PageType.Search.ToString();
		    } 
	    }

	    /// <summary>
	    /// Private field: Know in seconds how much time a database query is.
	    /// </summary>
	    private double _elapsedSeconds;

	    /// <summary>
	    /// Know in seconds how much time a database query is. Rounded by 3 decimals
	    /// </summary>
        public double ElapsedSeconds
        {
		    get { return Math.Round(_elapsedSeconds, 4); }
            set
            {
                _elapsedSeconds = value - value % 0.001;
            }
        }
	    
	    /// <summary>
	    /// Used to know how old the search query is
	    /// Used to know if a page is cached
	    /// </summary>
	    public double Offset =>   Math.Round(Math.Abs((DateTime.Now - _dateTime).TotalSeconds),2);


		/// <summary>
		/// Private field: Search Operator, and or OR
		/// </summary>
		private List<bool>? _searchOperatorOptions;

	    /// <summary>
	    /// Add to list in model (&amp;&amp;|| operators) true=&amp;&amp; false=||
	    /// </summary>
	    /// <param name="andOrChar"></param>
	    /// <param name="relativeLocation"></param>
	    public void SetAndOrOperator(char andOrChar, int relativeLocation = 0)
		{
			_searchOperatorOptions ??= new List<bool>();

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

	    /// <summary>
	    /// When using OR || statements, skip to next item in the for loop
	    /// false = (skip( continue to next item))
	    /// </summary>
	    /// <param name="indexer"></param>
	    /// <param name="max"></param>
	    /// <returns>false = (skip( continue to next item))</returns>
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
	    /// Copy the current object in memory
	    /// </summary>
	    /// <returns></returns>
	    public SearchViewModel Clone()
	    {
		    return (SearchViewModel) MemberwiseClone();
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
		    
		    Regex inUrlRegex = new Regex("[\\w!]+|([\"\'])(\\\\?.)*?\\1",
			    RegexOptions.IgnoreCase);

		    // Escape special quotes
		    defaultQuery = Regex.Replace(defaultQuery, "[“”‘’]", "\"");
		    
		    var regexInUrlMatches = inUrlRegex.Matches(defaultQuery);

		    foreach ( Match regexInUrl in regexInUrlMatches )
		    {
			    if ( string.IsNullOrEmpty(regexInUrl.Value) ) continue;

			    if ( regexInUrl.Value.Length <= 2 )
			    {
				    SetAddSearchForOptions("=");
				    SetAddSearchFor(regexInUrl.Value);
				    SetAddSearchInStringType("tags");
				    continue;
			    }

			    // Need to have the type registered in FileIndexPropList

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

			    // Detecting Not Queries (string must be at least 3 chars)
			    if ( ( regexInUrl.Index - 1 >= 0 && defaultQuery[regexInUrl.Index - 1] == '-' ) 
			         || ( regexInUrl.Index + 2 <= regexInUrl.Length  && defaultQuery[regexInUrl.Index + 2] == '-' ) )
				{
					SetAddSearchForOptions("-");
					continue;
				}
			    SetAddSearchForOptions("=");
			}

		    // fallback situation
		    // search on for example: '%'
		    if ( !SearchFor.Any() ) 
		    {
			    SetAddSearchFor(defaultQuery);
			    SetAddSearchInStringType("tags");
			    SetAddSearchForOptions("=");
			    return string.Empty;
		    }

		    // Regex: for ||&& without escape chars 
			//	// &&|\|\|
		    Regex andOrRegex = new Regex("&&|\\|\\|",
			    RegexOptions.IgnoreCase);
		    
		    var andOrRegexMatches = andOrRegex.Matches(defaultQuery);

		    foreach ( Match andOrValue in andOrRegexMatches )
		    {
			    SetAndOrOperator(AndOrRegex(andOrValue.Value));
		    }

		    // add for default situations
		    if ( SearchFor.Count != SearchOperatorOptions.Count )
		    {
			    for ( int i = SearchOperatorOptions.Count; i < SearchFor.Count; i++ )
			    {
				    SetAndOrOperator(AndOrRegex("&&"));
			    }
		    }
		    
	    
		    return returnQueryBuilder.ToString();
	    }
	    
	    
		/// <summary>
	    /// Filter for WideSearch
	    /// Always after wideSearch 
	    /// </summary>
	    /// <param name="model"></param>
	    /// <returns></returns>
	    public SearchViewModel NarrowSearch(SearchViewModel model)
	    {
		    if ( model.FileIndexItems == null ) model = new SearchViewModel();

		    for ( var i = 0; i < model.SearchIn.Count; i++ )
		    {
			    var propertyStringName = FileIndexItem.FileIndexPropList().FirstOrDefault(p =>
				    string.Equals(p, model.SearchIn[i], StringComparison.InvariantCultureIgnoreCase));
			    if ( string.IsNullOrEmpty(propertyStringName) ) continue;

			    var property = new FileIndexItem().GetType().GetProperty(propertyStringName)!;
					    
			    // skip OR searches
			    if ( !model.SearchOperatorContinue(i, model.SearchIn.Count) )
			    {
				    continue;
			    }
			    PropertySearch(model, property, model.SearchFor[i],model.SearchForOptions[i]);
		    }

		    return model;
	    }

	    /// <summary>
	    /// Search in properties by 
	    /// </summary>
	    /// <param name="model">to search in this model</param>
	    /// <param name="property">the property name to search in</param>
	    /// <param name="searchForQuery">the query to search for (always string) </param>
	    /// <param name="searchType">greater then, equal</param>
	    /// <returns>search values</returns>
	    private static SearchViewModel PropertySearch(SearchViewModel model, PropertyInfo property, string searchForQuery, SearchForOptionType searchType)
	    {

		    if ( property.PropertyType == typeof(string) )
		    {
			    switch (searchType)
			    {
				    case SearchForOptionType.Not:
					    model.FileIndexItems = model.FileIndexItems?.Where(
						    p => p.GetType().GetProperty(property.Name)?.Name == property.Name 
						         && ! // not
							         p.GetType().GetProperty(property.Name)!.GetValue(p, null)!
								         .ToString()!.ToLowerInvariant().Contains(searchForQuery)  
					    ).ToList();
					    break;
				    default:
					    model.FileIndexItems = model.FileIndexItems?.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name 
					                                                           && p.GetType().GetProperty(property.Name)!.GetValue(p, null)!
						                                                           .ToString()!.ToLowerInvariant().Contains(searchForQuery)  
					    ).ToList();
					    break;
			    }
			
			    return model;
		    }

		    if ( property.PropertyType == typeof(bool) )
		    {
			    bool.TryParse(searchForQuery, out var boolIsValue);
			    model.FileIndexItems = model.FileIndexItems?.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name 
			                                                           && (bool?) p.GetType().GetProperty(property.Name)?.GetValue(p, null)  == boolIsValue
			    ).ToList();
			    return model;
		    }
		    
		    if ( property.PropertyType == typeof(ExtensionRolesHelper.ImageFormat) )
		    {
			    
			    Enum.TryParse<ExtensionRolesHelper.ImageFormat>(
				    searchForQuery.ToLowerInvariant(), out var castImageFormat);

			    switch (searchType)
			    {
				    case SearchForOptionType.Not:
					    model.FileIndexItems = model.FileIndexItems?.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name 
					                                                           && (ExtensionRolesHelper.ImageFormat) p.GetType().GetProperty(property.Name)?
						                                                           .GetValue(p, null)!  
					                                                           != // not
					                                                           castImageFormat
					    ).ToList();
					    break;
				    default:
					    model.FileIndexItems = model.FileIndexItems?.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name 
					                                                           && (ExtensionRolesHelper.ImageFormat) p.GetType().GetProperty(property.Name)?
						                                                           .GetValue(p, null)!  == castImageFormat
					    ).ToList();
					    break;
			    }
			    return model;
		    }
		    
		    if ( property.PropertyType == typeof(DateTime) )
		    {
			    
			    var parsedDateTime = ParseDateTime(searchForQuery);
						
			    switch (searchType)
			    {
				    case SearchForOptionType.LessThen:
					    model.FileIndexItems = model.FileIndexItems?.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name 
					                                                           && (DateTime) p.GetType().GetProperty(property.Name)?.GetValue(p, null)! 
					                                                           <= parsedDateTime
					    ).ToList();
					    break;
				    case SearchForOptionType.GreaterThen:
					    model.FileIndexItems = model.FileIndexItems?.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name 
					                                                           && (DateTime) p.GetType().GetProperty(property.Name)?.GetValue(p, null)!
					                                                           >= parsedDateTime
					    ).ToList();
					    break;
				    default:
					    model.FileIndexItems = model.FileIndexItems?.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name 
					                                                           && (DateTime) p.GetType().GetProperty(property.Name)?.GetValue(p, null)!
					                                                           == parsedDateTime
					    ).ToList();
					    break;
			    }
			    
			    return model;
		    }


		    return model;
	    }
	    
	    
	    
	    /// <summary>
	    /// Internal API: to parse datetime objects
	    /// </summary>
	    /// <param name="input"></param>
	    /// <returns></returns>
	    public static DateTime ParseDateTime(string input)
	    {

		    // For relative values
		    if ( Regex.IsMatch(input, @"^\d+$") )
		    {
			    int.TryParse(input, out var relativeValue);
			    if(relativeValue >= 1) relativeValue = relativeValue * -1; // always in the past
			    if ( relativeValue > -60000 ) // 24-11-1854
			    {
				    return DateTime.Today.AddDays(relativeValue);
			    }
		    }
	        
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
    }
}
