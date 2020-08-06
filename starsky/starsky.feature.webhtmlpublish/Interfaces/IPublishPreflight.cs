using System.Collections.Generic;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IPublishPreflight
	{
		string GetNameConsole(string inputPath, IReadOnlyList<string> args);
	}
}
