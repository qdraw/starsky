using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.feature.rename.Models;

/// <summary>
///     Parses and applies DateTime token patterns for batch rename operations.
///     Supported tokens:
///     - dd: Day of month (01-31)
///     - MM: Month (01-12)
///     - yyyy: Year as 4-digit number
///     - HH: Hour (00-23)
///     - mm: Minute (00-59)
///     - ss: Second (00-59)
///     - {filenamebase}: Original filename without extension
///     - {seqn}: Sequence number (appended as -N when duplicates exist)
///     - .ext: File extension
///     - \\: Double backslash to escape tokens (e.g., \\d\\d for literal "dd")
/// </summary>
public partial class RenameTokenPattern
{
	private const string SequenceToken = "{seqn}";
	private readonly HashSet<string> _errors = [];
	private readonly string _pattern;

	private readonly HashSet<string> _validBracedTokens = new(StringComparer.Ordinal)
	{
		"{filenamebase}",
		"{seqn}",
		"{yyyy}",
		"{MM}",
		"{dd}",
		"{HH}",
		"{mm}",
		"{ss}",
		"{ext}"
	};

	public RenameTokenPattern(string pattern)
	{
		_pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
		ValidatePattern();
	}

	/// <summary>
	///     Get validation errors from pattern parsing
	/// </summary>
	public IReadOnlyCollection<string> Errors => _errors;

	/// <summary>
	///     Check if pattern is valid (no validation errors)
	/// </summary>
	public bool IsValid => _errors.Count == 0;

	/// <summary>
	///     Generate a new filename from the pattern, applying token substitution
	/// </summary>
	/// <param name="fileIndexItem">Source file metadata</param>
	/// <param name="sequenceNumber">Optional sequence number for duplicates (0 if none)</param>
	/// <returns>New filename with extension</returns>
	public string GenerateFileName(FileIndexItem fileIndexItem, int sequenceNumber = 0)
	{
		if ( string.IsNullOrEmpty(fileIndexItem.FileName) )
		{
			throw new ArgumentNullException(nameof(fileIndexItem));
		}

		var dateTime = fileIndexItem.DateTime;
		var originalFileName = FilenamesHelper.GetFileNameWithoutExtension(fileIndexItem.FileName);
		var originalExtension = FilenamesHelper.GetFileExtensionWithoutDot(fileIndexItem.FileName);

		var result = _pattern;
		var hasSeqToken = _pattern.Contains(SequenceToken, StringComparison.Ordinal);

		// Apply datetime tokens (only when not escaped)
		result = ReplaceUnescapedToken(result, "{yyyy}", dateTime.Year.ToString("D4"));
		result = ReplaceUnescapedToken(result, "{MM}", dateTime.Month.ToString("D2"));
		result = ReplaceUnescapedToken(result, "{dd}", dateTime.Day.ToString("D2"));
		result = ReplaceUnescapedToken(result, "{HH}", dateTime.Hour.ToString("D2"));
		result = ReplaceUnescapedToken(result, "{mm}", dateTime.Minute.ToString("D2"));
		result = ReplaceUnescapedToken(result, "{ss}", dateTime.Second.ToString("D2"));

		// Apply filename and extension tokens
		result = ReplaceUnescapedToken(result, "{filenamebase}", originalFileName);
		result = ReplaceUnescapedToken(result, "{ext}", originalExtension);

		// Apply sequence number
		if ( sequenceNumber > 0 )
		{
			result = hasSeqToken
				? ReplaceUnescapedToken(result, SequenceToken,
					$"-{sequenceNumber}")
				: InsertSequenceBeforeExtension(result, sequenceNumber);
		}
		else if ( hasSeqToken )
		{
			result = ReplaceUnescapedToken(result, SequenceToken, string.Empty);
		}

		// Ensure filename is valid
		var fileName = FilenamesHelper.GetFileName(result);
		if ( !FilenamesHelper.IsValidFileName(fileName) )
		{
			throw new InvalidOperationException($"Generated filename is invalid: {fileName}");
		}

		return fileName;
	}

	private static string ReplaceUnescapedToken(string input, string token, string value)
	{
		var pattern = $@"(?<!\\){Regex.Escape(token)}";
		return Regex.Replace(input, pattern, value,
			RegexOptions.None,
			TimeSpan.FromMilliseconds(100));
	}

	private static string InsertSequenceBeforeExtension(string input, int sequenceNumber)
	{
		var extension = Path.GetExtension(input);
		if ( string.IsNullOrEmpty(extension) )
		{
			return $"{input}-{sequenceNumber}";
		}

		var fileNameWithoutExtension = input[..^extension.Length];
		return $"{fileNameWithoutExtension}-{sequenceNumber}{extension}";
	}

	/// <summary>
	///     Validate the pattern for syntax errors
	/// </summary>
	private void ValidatePattern()
	{
		if ( string.IsNullOrWhiteSpace(_pattern) )
		{
			_errors.Add("Pattern cannot be empty");
			return;
		}

		// Check for unescaped braces that don't match known tokens
		var bracedTokens = BracesTokenRegex().Matches(_pattern);
		foreach ( var token in bracedTokens.Select(match => match.Value)
			         .Where(token => !_validBracedTokens.Contains(token)) )
		{
			_errors.Add($"Unknown token: {token}");
		}
	}

	[GeneratedRegex(@"\{[^}]*\}")]
	private static partial Regex BracesTokenRegex();
}
