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
	private static partial Regex StructureRegex();
	
	/// <summary>
	///     To Check if the structure is valid
	/// </summary>
	/// <param name="structure"></param>
	internal static bool StructureCheck(string? structure)
	{
		return !string.IsNullOrEmpty(structure) && 
		       StructureRegex().Match(structure).Success;
	}
}
