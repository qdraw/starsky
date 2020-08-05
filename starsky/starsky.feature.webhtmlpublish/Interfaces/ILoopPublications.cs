using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface ILoopPublications
	{
		Task<bool> Render(List<FileIndexItem> fileIndexItemsList, string[] base64ImageArray,
			string publishProfileName);
	}
}
