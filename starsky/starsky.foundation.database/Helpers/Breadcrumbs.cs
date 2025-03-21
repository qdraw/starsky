using System.Collections.Generic;
using System.Text;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Helpers;

public static class Breadcrumbs
{
	private const string PathSeparatorAlwaysUnixStyle = "/";

	/// <summary>
	///     Breadcrumb returns a list of parent folders
	///     it does not contain the current folder
	/// </summary>
	/// <param name="filePath">subPath (unix style)</param>
	/// <returns>list of parent folders</returns>
	public static List<string> BreadcrumbHelper(string? filePath)
	{
		if ( filePath == null )
		{
			return [];
		}

		filePath = RemoveSlashFromEnd(filePath);
		filePath = ShouldPrefixWithSlash(filePath);

		var breadcrumb = new List<string>();

		var filePathArray = filePath.Split(PathSeparatorAlwaysUnixStyle.ToCharArray());

		var dir = 0;
		while ( dir < filePathArray.Length - 1 )
		{
			if ( string.IsNullOrEmpty(filePathArray[dir]) )
			{
				breadcrumb.Add(PathSeparatorAlwaysUnixStyle);
			}
			else
			{
				var itemStringBuilder = new StringBuilder();

				for ( var i = 0; i <= dir; i++ )
				{
					if ( !string.IsNullOrEmpty(filePathArray[i]) )
					{
						itemStringBuilder.Append(PathSeparatorAlwaysUnixStyle + filePathArray[i]);
					}
				}

				breadcrumb.Add(itemStringBuilder.ToString());
			}

			dir++;
		}

		return breadcrumb;
	}

	private static string RemoveSlashFromEnd(string filePath)
	{
		filePath = PathHelper.RemoveLatestBackslash(filePath) ?? string.Empty;
		if ( string.IsNullOrEmpty(filePath) )
		{
			filePath = PathSeparatorAlwaysUnixStyle;
		}

		return filePath;
	}

	private static string ShouldPrefixWithSlash(string filePath)
	{
		if ( filePath[0].ToString() != PathSeparatorAlwaysUnixStyle )
		{
			filePath = PathSeparatorAlwaysUnixStyle + filePath;
		}

		return filePath;
	}
}
