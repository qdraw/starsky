using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.feature.search.ViewModels
{
	[SuppressMessage("ReSharper", "ArrangeAccessorOwnerBody")]
	public class SearchViewModel
	{
		public SearchViewModel()
		{
			// init default values
			SearchIn ??= new List<string>();
			FileIndexItems ??= new List<FileIndexItem>();
			Breadcrumb ??= new List<string>();

			// to know how long a query takes
			_dateTime = DateTime.UtcNow;
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
		public string? SearchQuery { get; set; } = string.Empty;

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
		/// Contains an list of Database fields to search in.
		/// </summary>
		public List<string> SearchIn { get; private set; }


		/// <summary>
		/// In which database field the search query is needed
		/// </summary>
		/// <param name="value">Search field name e.g. Tags</param>
		public void SetAddSearchInStringType(string value)
		{
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			SearchIn ??= new List<string>();

			// use ctor to have an empty list
			var fileIndexPropList = FileIndexItem.FileIndexPropList();
			var fileIndexPropListIndex = fileIndexPropList.FindIndex
				(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
			if ( fileIndexPropListIndex != -1 )
			{
				SearchIn.Add(fileIndexPropList[fileIndexPropListIndex]);
			}
		}


		/// <summary>
		/// Private field: Search for the following value in using SearchFor inside: _searchIn
		/// </summary>
		internal List<string>? SearchForInternal = new();

		/// <summary>
		/// The values to search for, to know which field use the same indexer in _searchIn
		/// </summary>
		public List<string> SearchFor
		{
			// don't change it to 'SearchFor => _searchFor'
			get
			{
				return SearchForInternal ?? new List<string>();
			}
		}

		/// <summary>
		/// Add string to searchFor list
		/// </summary>
		/// <param name="value"></param>
		public void SetAddSearchFor(string value)
		{
			SearchForInternal ??= new List<string>();
			SearchForInternal.Add(value.Trim().ToLowerInvariant());
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
		/// Private field: Search Options &gt;, &lt;,=. (greater than sign, less than sign, equal sign)
		/// to know which field use the same indexer in _searchIn or _searchFor
		/// </summary>
		internal List<SearchForOptionType>? SearchForOptionsInternal;

		/// <summary>
		/// Search Options eg &gt;, &lt;, =. (greater than sign, less than sign, equal sign)
		/// to know which field use the same indexer in _searchIn or _searchFor
		/// </summary>
		public List<SearchForOptionType> SearchForOptions
		{
			get
			{
				return SearchForOptionsInternal ?? new List<SearchForOptionType>();
			}
		}

		/// <summary>
		/// Add first char of a string to _searchForOptions list
		/// </summary>
		/// <param name="value">searchFor option (e.g. =, &gt;, &lt; </param>
		public SearchForOptionType SetAddSearchForOptions(string value)
		{
			SearchForOptionsInternal ??= new List<SearchForOptionType>();

			switch ( value.Trim()[0] )
			{
				case '>':
					SearchForOptionsInternal.Add(SearchForOptionType.GreaterThen);
					return SearchForOptionType.GreaterThen;
				case '<':
					SearchForOptionsInternal.Add(SearchForOptionType.LessThen);
					return SearchForOptionType.LessThen;
				case '-':
					SearchForOptionsInternal.Add(SearchForOptionType.Not);
					return SearchForOptionType.Not;
				case '=':
					SearchForOptionsInternal.Add(SearchForOptionType.Equal);
					return SearchForOptionType.Equal;
				case ':':
					SearchForOptionsInternal.Add(SearchForOptionType.Equal);
					return SearchForOptionType.Equal;
				case ';':
					SearchForOptionsInternal.Add(SearchForOptionType.Equal);
					return SearchForOptionType.Equal;
			}
			return SearchForOptionType.Equal;
		}

		/// <summary>
		/// The type of page returns, (Search or Trash)
		/// </summary>
		public string PageType
		{
			get
			{
				if ( string.IsNullOrEmpty(SearchQuery) ) return PageViewType.PageType.Search.ToString();
				return SearchQuery == TrashKeyword.TrashKeywordString ? PageViewType.PageType.Trash.ToString()
					: PageViewType.PageType.Search.ToString();
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
		[SuppressMessage("Usage", "S6561: Avoid using DateTime.Now " +
								  "for benchmarking or timespan calculation operations.")]
		public double Offset => Math.Round(Math.Abs(( DateTime.UtcNow - _dateTime ).TotalSeconds), 2);


		/// <summary>
		/// Private field: Search Operator, and or OR
		/// </summary>
		internal List<bool>? SearchOperatorOptionsInternal;

		/// <summary>
		/// Add to list in model (&amp;&amp;|| operators) true=&amp;&amp; false=||
		/// </summary>
		/// <param name="andOrChar"></param>
		/// <param name="relativeLocation"></param>
		public void SetAndOrOperator(char andOrChar, int relativeLocation = 0)
		{
			SearchOperatorOptionsInternal ??= new List<bool>();

			bool andOrBool = andOrChar == '&';

			if ( char.IsWhiteSpace(andOrChar) )
			{
				andOrBool = false;
			}

			if ( SearchOperatorOptionsInternal.Count == 0 && andOrChar == '|' )
			{
				SearchOperatorOptionsInternal.Add(false);
			}

			// Store item on a different location in the List<T>
			if ( relativeLocation == 0 )
			{
				SearchOperatorOptionsInternal.Add(andOrBool);
			}
			else if ( SearchOperatorOptionsInternal.Count + relativeLocation <= -1 )
			{
				SearchOperatorOptionsInternal.Insert(0, andOrBool);
			}
			else
			{
				SearchOperatorOptionsInternal.Insert(SearchOperatorOptionsInternal.Count + relativeLocation, andOrBool);
			}

		}

		/// <summary>
		/// Search Operator, eg. || &amp;&amp;
		/// </summary>
		public List<bool> SearchOperatorOptions
		{
			get
			{
				return SearchOperatorOptionsInternal ?? new List<bool>();
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
			if ( SearchOperatorOptionsInternal == null ) return true;
			if ( indexer <= -1 || indexer > max ) return true;
			// for -Datetime=1 (03-03-2019 00:00:00-03-03-2019 23:59:59), this are two queries >= fail!!
			if ( indexer >= SearchOperatorOptionsInternal.Count ) return true; // used when general words without update 
			var returnResult = SearchOperatorOptionsInternal[indexer];
			return returnResult;
		}

		/// <summary>
		/// ||[OR] = |, else = &amp;, default = string.Emphy 
		/// </summary>
		/// <param name="item">searchquery</param>
		/// <returns>bool</returns>
		public static char AndOrRegex(string item)
		{
			// (\|\||\&\&)$
			Regex rgx = new Regex(@"(\|\||\&\&)$", RegexOptions.IgnoreCase,
				TimeSpan.FromMilliseconds(100));

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
			return ( SearchViewModel )MemberwiseClone();
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

			(defaultQuery, returnQueryBuilder) = ParseQuotedValues(defaultQuery, returnQueryBuilder);

			// fallback situation
			// search on for example: '%'
			if ( SearchFor.Count == 0 )
			{
				SetAddSearchFor(defaultQuery);
				SetAddSearchInStringType("tags");
				SetAddSearchForOptions("=");
				return string.Empty;
			}

			// Regex: for ||&& without escape chars 
			//	// &&|\|\|
			Regex andOrRegex = new Regex("&&|\\|\\|",
				RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

			var andOrRegexMatches = andOrRegex.Matches(defaultQuery);

			foreach ( Match andOrValue in andOrRegexMatches )
			{
				SetAndOrOperator(AndOrRegex(andOrValue.Value));
			}

			// add for default situations
			if ( SearchFor.Count != SearchOperatorOptions.Count )
			{
				for ( var i = SearchOperatorOptions.Count; i < SearchFor.Count; i++ )
				{
					SetAndOrOperator(AndOrRegex("&&"));
				}
			}

			return returnQueryBuilder.ToString();
		}

		private (string defaultQuery, StringBuilder returnQueryBuilder)
			ParseQuotedValues(string defaultQuery, StringBuilder returnQueryBuilder)
		{
			// Get Quoted values
			// (["'])(\\?.)*?\1

			// Quoted or words
			// [\w!]+|(["'])(\\?.)*?\1

			Regex inUrlRegex = new Regex("[\\w!]+|([\"\'])(\\\\?.)*?\\1",
				RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));

			// Escape special quotes
			defaultQuery = Regex.Replace(defaultQuery, "[“”‘’]", "\"",
				RegexOptions.None, TimeSpan.FromMilliseconds(100));

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
				var lastChar = defaultQuery[startIndexer + regexInUrl.Length - 1];

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
					 || ( regexInUrl.Index + 2 <= regexInUrl.Length && defaultQuery[regexInUrl.Index + 2] == '-' ) )
				{
					SetAddSearchForOptions("-");
					continue;
				}
				SetAddSearchForOptions("=");
			}

			return (defaultQuery, returnQueryBuilder);
		}

		/// <summary>
		/// Filter for WideSearch
		/// Always after wideSearch
		/// Hides by default xmp sidecar files
		/// </summary>
		/// <param name="model">model to make finer</param>
		/// <returns>complete result</returns>
		public static SearchViewModel NarrowSearch(SearchViewModel model)
		{
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if ( model.FileIndexItems == null ) model = new SearchViewModel();

			for ( var i = 0; i < model.SearchIn.Count; i++ )
			{
				var propertyStringName = FileIndexItem.FileIndexPropList().Find(p =>
						string.Equals(p, model.SearchIn[i],
							StringComparison.InvariantCultureIgnoreCase));

				if ( string.IsNullOrEmpty(propertyStringName) )
				{
					continue;
				}

				var property = new FileIndexItem().GetType().GetProperty(propertyStringName)!;

				// skip OR searches
				if ( !model.SearchOperatorContinue(i, model.SearchIn.Count) )
				{
					continue;
				}
				PropertySearch(model, property, model.SearchFor[i], model.SearchForOptions[i]);
			}

			// hide xmp files in default view
			if ( model.SearchIn.TrueForAll(p => !string.Equals(p, nameof(SearchInTypes.imageformat),
					StringComparison.InvariantCultureIgnoreCase)) )
			{
				model.FileIndexItems = model.FileIndexItems!
					.Where(p => p.ImageFormat != ExtensionRolesHelper.ImageFormat.xmp).ToList();
			}

			return model;
		}

		internal static SearchViewModel PropertySearchStringType(
			SearchViewModel model,
			PropertyInfo property, string searchForQuery,
			SearchForOptionType searchType)
		{
			switch ( searchType )
			{
				case SearchForOptionType.Not:
					model.FileIndexItems = model.FileIndexItems!.Where(
						p => p.GetType().GetProperty(property.Name)?.Name == property.Name
							 && ! // not
								 p.GetType().GetProperty(property.Name)!.GetValue(p, null)?
									 .ToString()?.ToLowerInvariant().Contains(searchForQuery,
										 StringComparison.InvariantCultureIgnoreCase) == true
					).ToList();
					break;
				default:
					model.FileIndexItems = model.FileIndexItems?
						.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name
									&& p.GetType().GetProperty(property.Name)!.GetValue(p, null)?
										.ToString()?.ToLowerInvariant().Contains(searchForQuery,
											StringComparison.InvariantCultureIgnoreCase) == true
						).ToList();
					break;
			}

			return model;
		}

		internal static SearchViewModel PropertySearchBoolType(
			SearchViewModel? model,
			PropertyInfo? property, bool boolIsValue)
		{
			if ( model == null )
			{
				return new SearchViewModel();
			}

			if ( property == null )
			{
				return model;
			}

			model.FileIndexItems = model.FileIndexItems?
				.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name
							&& ( bool? )p.GetType().GetProperty(property.Name)?.GetValue(p, null) == boolIsValue
				).ToList();
			return model;
		}

		private static SearchViewModel PropertySearchImageFormatType(
			SearchViewModel model,
			PropertyInfo property, ExtensionRolesHelper.ImageFormat castImageFormat,
			SearchForOptionType searchType)
		{
			switch ( searchType )
			{
				case SearchForOptionType.Not:
					model.FileIndexItems = model.FileIndexItems!
						.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name
									&& ( ExtensionRolesHelper.ImageFormat )p.GetType().GetProperty(property.Name)?
										.GetValue(p, null)!
									!= // not
									castImageFormat
						).ToList();
					break;
				default:
					model.FileIndexItems = model.FileIndexItems!
						.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name
									&& ( ExtensionRolesHelper.ImageFormat )p.GetType().GetProperty(property.Name)?
										.GetValue(p, null)! == castImageFormat
						).ToList();
					break;
			}
			return model;
		}

		private static SearchViewModel PropertySearchDateTimeType(
			SearchViewModel model,
			PropertyInfo property,
			string searchForQuery,
			SearchForOptionType searchType)
		{
			var parsedDateTime = ParseDateTime(searchForQuery);

			switch ( searchType )
			{
				case SearchForOptionType.LessThen:
					model.FileIndexItems = model.FileIndexItems!
						.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name
									&& ( DateTime )p.GetType().GetProperty(property.Name)?.GetValue(p, null)!
									<= parsedDateTime
						).ToList();
					break;
				case SearchForOptionType.GreaterThen:
					model.FileIndexItems = model.FileIndexItems!
						.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name
									&& ( DateTime )p.GetType().GetProperty(property.Name)?.GetValue(p, null)!
									>= parsedDateTime
						).ToList();
					break;
				default:
					model.FileIndexItems = model.FileIndexItems!
						.Where(p => p.GetType().GetProperty(property.Name)?.Name == property.Name
									&& ( DateTime )p.GetType().GetProperty(property.Name)?.GetValue(p, null)!
									== parsedDateTime
						).ToList();
					break;
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
		internal static SearchViewModel PropertySearch(SearchViewModel model,
			PropertyInfo property, string searchForQuery, SearchForOptionType searchType)
		{

			if ( property.PropertyType == typeof(string) )
			{
				return PropertySearchStringType(model, property, searchForQuery, searchType);
			}

			if ( ( property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?) ) &&
				 bool.TryParse(searchForQuery, out var boolIsValue) )
			{
				return PropertySearchBoolType(model, property, boolIsValue);
			}

			if ( property.PropertyType == typeof(ExtensionRolesHelper.ImageFormat) &&
				 Enum.TryParse<ExtensionRolesHelper.ImageFormat>(
					searchForQuery.ToLowerInvariant(), out var castImageFormat) )
			{
				return PropertySearchImageFormatType(model, property, castImageFormat, searchType);
			}

			if ( property.PropertyType == typeof(DateTime) )
			{
				return PropertySearchDateTimeType(model, property, searchForQuery, searchType);
			}

			return model;
		}

		/// <summary>
		/// Internal API: to parse datetime objects
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		internal static DateTime ParseDateTime(string input)
		{

			// For relative values
			if ( Regex.IsMatch(input, @"^\d+$",
					 RegexOptions.None, TimeSpan.FromMilliseconds(100)) &&
				 int.TryParse(input, out var relativeValue) )
			{
				if ( relativeValue >= 1 ) relativeValue *= -1; // always in the past
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

			foreach ( var pattern in patternLab )
			{
				DateTime.TryParseExact(input,
					pattern,
					CultureInfo.InvariantCulture,
					DateTimeStyles.None, out dateTime);
				if ( dateTime.Year > 2 ) return dateTime;
			}
			return dateTime.Year > 2 ? dateTime : DateTime.Now;
		}
	}
}
