using System;
using System.Collections.Generic;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models.Structure;

namespace starsky.foundation.platform.Models;

public class AppSettingsStructureModel(string? defaultPattern = null)
{
	private const string GenericDefaultPattern =
		"/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

	/// <summary>
	///     Internal Structure save location
	/// </summary>
	private string? _defaultPattern = defaultPattern;

	public string DefaultPattern
	{
		get => string.IsNullOrEmpty(_defaultPattern) ? GenericDefaultPattern : _defaultPattern;
		//   - dd 	            The day of the month, from 01 through 31.
		//   - MM 	            The month, from 01 through 12.
		//   - yyyy 	        The year as a four-digit number.
		//   - HH 	            The hour, using a 24-hour clock from 00 to 23.
		//   - mm 	            The minute, from 00 through 59.
		//   - ss 	            The second, from 00 through 59.
		//   - https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
		//   - \\               (double escape sign or double backslash); to escape dd use this: \\d\\d 
		//   - /                (slash); is split in folder (Windows / Linux / Mac)
		//   - .ext             (dot ext); extension for example: .jpg
		//   - {filenamebase}   use the orginal filename without extension
		//   - *                (asterisk); match anything
		//   - *starksy*        Match the folder match that contains the word 'starksy'
		//    Please update /starskyimportercli/readme.md when this changes
		set // using Json importer
		{
			if ( string.IsNullOrEmpty(value) || value == "/" )
			{
				return;
			}

			var structure = PathHelper.PrefixDbSlash(value);
			_defaultPattern = PathHelper.RemoveLatestBackslash(structure);
			// Structure regex check
			StructureCheck(_defaultPattern);
		}
	}

	public List<StructureRule> Rules { get; set; } = new();

	/// <summary>
	///     To Check if the structure is any good
	/// </summary>
	/// <param name="structure"></param>
	/// <exception cref="ArgumentException"></exception>
	public static void StructureCheck(string? structure)
	{
		if ( string.IsNullOrEmpty(structure) )
		{
			throw new ArgumentNullException(structure, "(StructureCheck) Structure is empty");
		}

		if ( StructureRegexHelper.StructureRegex().Match(structure).Success )
		{
			return;
		}

		throw new ArgumentException("(StructureCheck) Structure is not confirm regex - " +
		                            structure);
	}
}

public class StructureRule
{
	private string _pattern = string.Empty;
	public StructureRuleConditions Conditions { get; set; } = new();

	public string Pattern
	{
		get => _pattern;
		set
		{
			if ( StructureRegexHelper.StructureRegex().Match(value).Success )
			{
				_pattern = value;
			}
		}
	}
}

public class StructureRuleConditions
{
	public List<ExtensionRolesHelper.ImageFormat> ImageFormats { get; set; } = [];
}
