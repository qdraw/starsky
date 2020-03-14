using System;
using System.Globalization;
using System.Linq.Expressions;
using starsky.foundation.database.Models;
using starskycore.Models;
using starskycore.ViewModels;

namespace starskycore.Services
{
	public class SearchWideDateTime
	{

		/// <summary>
	    /// Query for DateTime: in between values, entire days, from, type of queries
	    /// </summary>
	    /// <param name="sourceList">Query Source</param>
	    /// <param name="model">output</param>
	    /// <param name="indexer">number of search query (i)</param>
	    /// <param name="type"></param>
	    public Expression<Func<FileIndexItem,bool>> WideSearchDateTimeGet(SearchViewModel model, int indexer,  WideSearchDateTimeGetType type)
	    {
			SearchForEntireDay(model,indexer);

			// faster search for searching within
			// how ever this is still triggered multiple times
			var beforeIndexSearchForOptions =
				model.SearchForOptions.IndexOf(SearchViewModel.SearchForOptionType.GreaterThen);
			var afterIndexSearchForOptions =
				model.SearchForOptions.IndexOf(SearchViewModel.SearchForOptionType.LessThen);
			if ( beforeIndexSearchForOptions >= 0 &&
				 afterIndexSearchForOptions >= 0 )
			{
				var beforeDateTime =
					model.ParseDateTime(model.SearchFor[beforeIndexSearchForOptions]);
				
				var afterDateTime =
					model.ParseDateTime(model.SearchFor[afterIndexSearchForOptions]);

				// We have now an extra query, and this is always AND  
				model.SetAndOrOperator('&', -2);
				
				switch ( type )
				{
					case WideSearchDateTimeGetType.DateTime:
						return (p => p.DateTime >= beforeDateTime && p.DateTime <= afterDateTime);
					case WideSearchDateTimeGetType.LastEdited:
						return (p => p.LastEdited >= beforeDateTime && p.LastEdited <= afterDateTime);
					case WideSearchDateTimeGetType.AddToDatabase:
						return (p => p.AddToDatabase >= beforeDateTime && p.AddToDatabase <= afterDateTime);
					default:
						throw new ArgumentNullException("enum incomplete");
				}
			}
			
			var dateTime = model.ParseDateTime(model.SearchFor[indexer]);

			// Normal search
			switch ( model.SearchForOptions[indexer] )
			{
				case SearchViewModel.SearchForOptionType.LessThen:
					// "<":
					switch ( type )
					{
						case WideSearchDateTimeGetType.DateTime:
							return (p => p.DateTime <= dateTime);
						case WideSearchDateTimeGetType.LastEdited:
							return (p => p.LastEdited <= dateTime);
						case WideSearchDateTimeGetType.AddToDatabase:
							return (p => p.AddToDatabase <= dateTime);
						default:
							throw new ArgumentNullException(nameof(type));
					}
				case SearchViewModel.SearchForOptionType.GreaterThen:
					switch ( type )
					{
						case WideSearchDateTimeGetType.DateTime:
							return (p => p.DateTime >= dateTime);
						case WideSearchDateTimeGetType.LastEdited:
							return (p => p.LastEdited >= dateTime);
						case WideSearchDateTimeGetType.AddToDatabase:
							return (p => p.AddToDatabase >= dateTime);
						default:
							throw new ArgumentNullException(nameof(type));
					}
				default:
					switch ( type )
					{
						case WideSearchDateTimeGetType.DateTime:
							return (p => p.DateTime == dateTime);
						case WideSearchDateTimeGetType.LastEdited:
							return (p => p.LastEdited == dateTime);
						case WideSearchDateTimeGetType.AddToDatabase:
							return (p => p.AddToDatabase == dateTime);
						default:
							throw new ArgumentNullException(nameof(type));
					}
			}
	    }
		
		/// <summary>
		/// Convert 1 to today
		/// </summary>
		/// <param name="model">to add results to</param>
		/// <param name="indexer">in the index</param>
		private void SearchForEntireDay(SearchViewModel model, int indexer)
		{
			var dateTime = model.ParseDateTime(model.SearchFor[indexer]);
			
			model.SearchFor[indexer] = dateTime.ToString("dd-MM-yyyy HH:mm:ss",
				CultureInfo.InvariantCulture);
			
			// Searching for entire day
			if ( model.SearchForOptions[indexer] != SearchViewModel.SearchForOptionType.Equal ||
			     dateTime.Hour != 0 || dateTime.Minute != 0 || dateTime.Second != 0 ||
			     dateTime.Millisecond != 0 ) return;
			
			model.SearchForOptions[indexer] = SearchViewModel.SearchForOptionType.GreaterThen;
			model.SearchForOptions.Add(SearchViewModel.SearchForOptionType.LessThen);

			var add24Hours = dateTime.AddHours(23)
				.AddMinutes(59).AddSeconds(59)
				.ToString(CultureInfo.InvariantCulture);
			model.SearchFor.Add(add24Hours);
			model.SearchIn.Add("DateTime");
		}
		
		/// <summary>
		/// Static binded types that are supported
		/// </summary>
		public enum WideSearchDateTimeGetType
		{
			DateTime,
			LastEdited,
			AddToDatabase
		}
	}
}
