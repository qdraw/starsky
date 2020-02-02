using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.Health
{
	public class PathExistOptions
	{
		internal List<string> ConfiguredPaths { get; } = new List<string>();

		public void AddPath(string fullFilePath)
		{
			ConfiguredPaths.Add(fullFilePath);
		}

	}
}
