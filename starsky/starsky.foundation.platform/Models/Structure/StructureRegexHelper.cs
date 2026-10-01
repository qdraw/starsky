using System.Linq;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Models.Structure;

public static partial class StructureRegexHelper
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
	public static bool StructureCheck(string? structure)
	{
		return !string.IsNullOrEmpty(structure) &&
		       StructureRegex().IsMatch(structure) &&
		       // a structure is a relative sub path, it should not climb out of the storage folder
		       !structure.Split('/', '\\').Any(segment => segment.Trim() == "..");
	}
}
