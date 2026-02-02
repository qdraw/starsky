using System.Collections.Generic;
using System.Text.RegularExpressions;
using starsky.feature.rename.DateTimeRepair.Models;

namespace starsky.feature.rename.DateTimeRepair.Helpers;

public static partial class DateTimePatterns
{
	/// <summary>
	///     Common datetime patterns in filenames
	/// </summary>
	internal static readonly List<DateTimePattern> DateTimePatternList =
	[
		// YYYYMMDD_HHMMSS
		new()
		{
			Regex = yyyyMMdd_HHmmssRegex(),
			Format = "yyyyMMdd_HHmmss",
			Description = "YYYYMMDD_HHMMSS"
		},
		// YYYY-MM-DD_HH-MM-SS
		new()
		{
			Regex = YYYYdashMMdashDD_HHdashMMdashSSRegex(),
			Format = "yyyy-MM-dd_HH-mm-ss",
			Description = "YYYY-MM-DD_HH-MM-SS"
		},
		// YYYYMMDD_HHMM
		new()
		{
			Regex = YYYYMMDD_HHMMRegex(),
			Format = "yyyyMMdd_HHmm",
			Description = "YYYYMMDD_HHMM"
		},
		// YYYYMMDD
		new() { Regex = YyyymmddRegex(), Format = "yyyyMMdd", Description = "YYYYMMDD" }
	];

	[GeneratedRegex(@"(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})",
		RegexOptions.None,
		100)]
	private static partial Regex yyyyMMdd_HHmmssRegex();

	[GeneratedRegex(@"(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})",
		RegexOptions.None,
		100)]
	private static partial Regex YYYYdashMMdashDD_HHdashMMdashSSRegex();

	[GeneratedRegex(@"(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})",
		RegexOptions.None,
		100)]
	private static partial Regex YYYYMMDD_HHMMRegex();

	[GeneratedRegex(@"(\d{4})(\d{2})(\d{2})",
		RegexOptions.None,
		100)]
	private static partial Regex YyyymmddRegex();
}
