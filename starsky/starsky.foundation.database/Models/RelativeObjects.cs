using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using starsky.foundation.platform.Helpers;
#pragma warning disable CS8618

namespace starsky.foundation.database.Models
{
	/// <summary>
	/// In DetailView there are values added for handling Args  
	/// </summary>
	public sealed class RelativeObjects
	{
		/// <summary>
		/// Set Args based on Collections and ColorClass Settings
		/// </summary>
		/// <param name="collections"></param>
		/// <param name="colorClassActiveList"></param>
		public RelativeObjects(bool collections, List<ColorClassParser.Color>? colorClassActiveList)
		{
			if ( !collections )
			{
				Args.Add(nameof(collections).ToLowerInvariant(), "false");
			}

			if ( colorClassActiveList is { Count: >= 1 } )
			{
				var colorClassArg = new StringBuilder();
				for ( int i = 0; i < colorClassActiveList.Count; i++ )
				{
					var colorClass = colorClassActiveList[i];
					if ( i == colorClassActiveList.Count - 1 )
					{
						colorClassArg.Append(colorClass.GetHashCode());
					}
					else
					{
						colorClassArg.Append(colorClass.GetHashCode() + ",");
					}
				}
				Args.Add(nameof(FileIndexItem.ColorClass).ToLowerInvariant(), colorClassArg.ToString());
			}
		}

		public RelativeObjects()
		{
		}

		public string NextFilePath { get; set; }
		public string PrevFilePath { get; set; }

		public string NextHash { get; set; }

		public string PrevHash { get; set; }

		/// <summary>
		/// Private field
		/// </summary>
		private Dictionary<string, string> ArgsPrivate { get; set; } = new Dictionary<string, string>();

		/// <summary>
		/// Prevent overwrites with null args
		/// </summary>
		[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
		public Dictionary<string, string> Args
		{
			get { return ArgsPrivate; }
			set
			{
				if ( value == null )
				{
					return;
				}

				ArgsPrivate = value;
			}
		}


	}
}
