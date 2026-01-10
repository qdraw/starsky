using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
public class RenameTokenPattern
{
	private readonly string _pattern;
	private readonly HashSet<string> _errors = new();

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
		if ( fileIndexItem?.FileName == null )
		{
			throw new ArgumentNullException(nameof(fileIndexItem));
		}

		var dateTime = fileIndexItem.DateTime;
		var originalFileName = FilenamesHelper.GetFileNameWithoutExtension(fileIndexItem.FileName);
		var originalExtension = Path.GetExtension(fileIndexItem.FileName);

		var result = _pattern;
		var hasSeqToken = _pattern.Contains("{seqn}", StringComparison.Ordinal);

		// Apply datetime tokens (only when not escaped)
		result = ReplaceUnescapedToken(result, "yyyy", dateTime.Year.ToString("D4"));
		result = ReplaceUnescapedToken(result, "MM", dateTime.Month.ToString("D2"));
		result = ReplaceUnescapedToken(result, "dd", dateTime.Day.ToString("D2"));
		result = ReplaceUnescapedToken(result, "HH", dateTime.Hour.ToString("D2"));
		result = ReplaceUnescapedToken(result, "mm", dateTime.Minute.ToString("D2"));
		result = ReplaceUnescapedToken(result, "ss", dateTime.Second.ToString("D2"));

		// Apply filename and extension tokens
		result = ReplaceUnescapedToken(result, "{filenamebase}", originalFileName);
		result = ReplaceUnescapedToken(result, ".ext", originalExtension);

		// Apply sequence number
		if ( sequenceNumber > 0 )
		{
			if ( hasSeqToken )
			{
				result = ReplaceUnescapedToken(result, "{seqn}", $"-{sequenceNumber}");
			}
			else
			{
				result = InsertSequenceBeforeExtension(result, sequenceNumber);
			}
		}
		else if ( hasSeqToken )
		{
			result = ReplaceUnescapedToken(result, "{seqn}", string.Empty);
		}

		// Handle escaped tokens - unescape double backslashes
		result = UnescapeTokens(result);

		// Ensure filename is valid
		var fileName = Path.GetFileName(result);
		if ( !FilenamesHelper.IsValidFileName(fileName) )
		{
			throw new InvalidOperationException($"Generated filename is invalid: {fileName}");
		}

		return fileName;
	}

	private static string ReplaceUnescapedToken(string input, string token, string value)
	{
		var pattern = $@"(?<!\\){Regex.Escape(token)}";
		return Regex.Replace(input, pattern, value);
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
	///     Unescape double backslash sequences (e.g., \\d\\d becomes dd)
	/// </summary>
	private static string UnescapeTokens(string input)
	{
		// Replace escaped sequences: \\X becomes X (where X is any character)
		return Regex.Replace(input, @"\\(.)", "$1");
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
		var bracedTokens = Regex.Matches(_pattern, @"\{[^}]*\}");
		foreach ( Match match in bracedTokens )
		{
			var token = match.Value;
			if ( token != "{filenamebase}" && token != "{seqn}" )
			{
				_errors.Add($"Unknown token: {token}");
			}
		}

		// Basic check for balanced escaping
		var unescapedBackslashes = Regex.Matches(_pattern, @"(?<!\\)\\(?!\\)");
		if ( unescapedBackslashes.Count > 0 )
		{
			_errors.Add("Invalid escape sequence: use \\\\ (double backslash) to escape");
		}
	}
}
