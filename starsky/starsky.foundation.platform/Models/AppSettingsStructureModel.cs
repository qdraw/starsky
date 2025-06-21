using System.Collections.Generic;
using System.Text.Json.Serialization;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models.Structure;

namespace starsky.foundation.platform.Models;

public class AppSettingsStructureModel
{
	private const string GenericDefaultPattern =
		"/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";

	/// <summary>
	///     Internal Structure save location
	/// </summary>
	private string? _defaultPattern;

	public AppSettingsStructureModel(string? defaultPattern = null)
	{
		SetDefaultPattern(defaultPattern);
	}

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
		set
			=>
				SetDefaultPattern(value);
	}

	public List<StructureRule> Rules { get; set; } = new();

	public HashSet<string> Errors { get; set; } = [];

	private void SetDefaultPattern(string? value)
	{
		if ( string.IsNullOrEmpty(value) || value == "/" )
		{
			return;
		}

		var structure = PathHelper.PrefixDbSlash(value);
		structure = PathHelper.RemoveLatestBackslash(structure);

		if ( StructureRegexHelper.StructureCheck(structure) )
		{
			_defaultPattern = structure;
			return;
		}

		if ( !string.IsNullOrEmpty(value) )
		{
			Errors.Add($"Structure '{structure}' is not valid");
		}
	}

	/// <summary>
	///     Feature to overwrite structures when importing using a header
	///     Overwrite the structure in the ImportIndexItem
	/// </summary>
	/// <param name="overwriteStructure">overwrite structure</param>
	/// <param name="importSettingsStructureErrors"></param>
	public void OverrideDefaultPatternAndDisableRules(string overwriteStructure,
		IReadOnlyList<string> importSettingsStructureErrors)
	{
		if ( importSettingsStructureErrors.Count >= 1 )
		{
			foreach ( var error in importSettingsStructureErrors )
			{
				Errors.Add(error);
			}
		}

		if ( string.IsNullOrWhiteSpace(overwriteStructure) )
		{
			return;
		}

		DefaultPattern = overwriteStructure;
		Rules = [];
	}

	/// <summary>
	///     Clone the structure from appSettings, to avoid referenced objects
	/// </summary>
	/// <returns></returns>
	public AppSettingsStructureModel Clone()
	{
		return this.CloneViaJson() ?? new AppSettingsStructureModel();
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
			if ( StructureRegexHelper.StructureCheck(value) )
			{
				_pattern = value;
			}
			else
			{
				Errors.Add($"Structure '{value}' is not valid");
			}
		}
	}

	public List<string> Errors { get; set; } = [];
}

public class StructureRuleConditions
{
	[JsonConverter(typeof(EnumListConverter<ExtensionRolesHelper.ImageFormat>))]
	public List<ExtensionRolesHelper.ImageFormat> ImageFormats { get; set; } = [];

	public string Source { get; set; } = string.Empty;
}
