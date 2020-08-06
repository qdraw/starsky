using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IWebHtmlPublishService
	{
		Task<bool> RenderCopy(List<FileIndexItem> fileIndexItemsList, 
			string[] base64ImageArray, string publishProfileName, 
			string outputFullFilePath, bool moveSourceFiles = false);
	}
}
