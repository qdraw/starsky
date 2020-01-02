using System.Text.RegularExpressions;

namespace starskycore.Helpers
{
	public class FilenamesHelper
	{
				
		public bool IsValidFileName(string filename)
		{
			// use the same as in the front-end
			var extensionRegex =
				new Regex("^[a-zA-Z0-9_](?:[a-zA-Z0-9 ._-]*[a-zA-Z0-9])?\\.[a-zA-Z0-9_-]+$",
					RegexOptions.CultureInvariant);

			return extensionRegex.IsMatch(filename);
		}

		/// <summary>
		/// Get the filename from a filepath
		/// https://stackoverflow.com/a/40635378
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		public string GetFileName(string filepath)
		{
			// unescaped regex:
			// [^\/]+(?=\.[\w]+\.$)|[^\/]+$
			var extensionRegex =
				new Regex("[^\\/]+(?=\\.[\\w]+\\.$)|[^\\/]+$",
					RegexOptions.CultureInvariant);
			return extensionRegex.Match(filepath).Value;
		}
	}
}
