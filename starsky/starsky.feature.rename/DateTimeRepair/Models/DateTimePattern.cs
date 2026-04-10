using System.Text.RegularExpressions;

namespace starsky.feature.rename.DateTimeRepair.Models;

/// <summary>
///     Internal class for datetime pattern definition
/// </summary>
internal sealed class DateTimePattern
{
	public required Regex Regex { get; init; }
	public required string Format { get; init; }
	public required string Description { get; init; }
}
