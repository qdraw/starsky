using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Models.Structure;

internal static partial class StructureRegexHelper
{
	/// <summary>
	///     Unescaped regex:
	///     ^(\/.+)?\/([\/_ A-Z0-9*{}\.\\-]+(?=\.ext))\.ext$
	/// </summary>
	/// <returns></returns>
	[GeneratedRegex(@"^(\/.+)?\/([\/_ A-Z0-9*{}\.\\-]+(?=\.ext))\.ext$", RegexOptions.IgnoreCase,
		300)]
	internal static partial Regex StructureRegex();
}
