using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Models;

namespace starsky.foundation.storage.Structure.Helpers;

public class ParseDateTimeFromFileNameHelper(AppSettingsStructureModel settingsStructure)
{
	public DateTime ParseDateTimeFromFileName(StructureInputModel inputModel)
	{
		var structure = StructureService.GetStructureSetting(settingsStructure, inputModel);

		// Depends on 'AppSettingsProvider.Structure'
		// depends on SourceFullFilePath
		if ( string.IsNullOrEmpty(inputModel.FileNameBase) )
		{
			return new DateTime(0, DateTimeKind.Utc);
		}

		// Replace asterisk > escape all options
		var structuredFileName = structure.Split("/".ToCharArray()).LastOrDefault();
		if ( structuredFileName == null || string.IsNullOrEmpty(inputModel.FileNameBase) )
		{
			return new DateTime(0, DateTimeKind.Utc);
		}

		structuredFileName = structuredFileName.Replace("*", "");
		structuredFileName = structuredFileName.Replace(".ext", string.Empty);
		structuredFileName = structuredFileName.Replace("{filenamebase}", string.Empty);

		DateTime.TryParseExact(inputModel.FileNameBase,
			structuredFileName,
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out var dateTime);

		if ( dateTime.Year >= 2 )
		{
			return dateTime;
		}

		// Now retry it and replace special charaters from string
		// For parsing files like: '2018-08-31 18.50.35' > '20180831185035'
		var pattern = new Regex("-|_| |;|\\.|:",
			RegexOptions.None, TimeSpan.FromMilliseconds(100));

		var fileName = pattern.Replace(inputModel.FileNameBase, string.Empty);
		structuredFileName = pattern.Replace(structuredFileName, string.Empty);

		DateTime.TryParseExact(fileName,
			structuredFileName,
			CultureInfo.InvariantCulture,
			DateTimeStyles.None,
			out dateTime);

		if ( dateTime.Year >= 2 )
		{
			return dateTime;
		}

		// when using /yyyymmhhss_{filenamebase}.jpg
		// For the situation that the image has no exif date and there is an appendix used (in the config)
		if ( !string.IsNullOrWhiteSpace(fileName) && structuredFileName.Length >= fileName.Length )
		{
			 structuredFileName = structuredFileName.Substring(0, fileName.Length - 1);

			DateTime.TryParseExact(fileName,
				structuredFileName,
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out dateTime);
		}

		if ( dateTime.Year >= 2 )
		{
			return dateTime;
		}

		if ( !string.IsNullOrWhiteSpace(fileName) && fileName.Length >= structuredFileName.Length )
		{
			var numericPattern = new Regex("[^0-9]", RegexOptions.None,
				TimeSpan.FromMilliseconds(1000));
			var fileName2 = numericPattern.Replace(fileName, string.Empty);
			
			DateTime.TryParseExact(fileName2,
				structuredFileName,
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out dateTime);
		}

		if ( dateTime.Year >= 2 )
		{
			return dateTime;
		}

		// For the situation that the image has no exif date and there is an appendix
		// used in the source filename AND the config
		if ( !string.IsNullOrEmpty(fileName) && fileName.Length >= structuredFileName.Length )
		{

			structuredFileName = RemoveEscapedCharacters(structuredFileName);

			// short the filename with structuredFileName
			fileName = fileName.Substring(0, structuredFileName.Length);

			DateTime.TryParseExact(fileName,
				structuredFileName,
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out dateTime);
		}

		// Return 0001-01-01 if everything fails
		return dateTime;
	}

	/// <summary>
	///     Removes the escaped characters and the first character after the backslash
	/// </summary>
	/// <param name="inputString">to input</param>
	/// <returns>the input string without those characters</returns>
	public static string RemoveEscapedCharacters(string inputString)
	{
		var newString = new StringBuilder();
		for ( var i = 0; i < inputString.ToCharArray().Length; i++ )
		{
			var structuredCharArray = inputString[i];
			var escapeChar = "\\"[0];
			if ( i != 0 && structuredCharArray != escapeChar && inputString[i - 1] != escapeChar )
			{
				newString.Append(structuredCharArray);
			}

			// add the first one
			if ( i == 0 && structuredCharArray != escapeChar )
			{
				newString.Append(structuredCharArray);
			}
		}

		return newString.ToString();
	}
}
