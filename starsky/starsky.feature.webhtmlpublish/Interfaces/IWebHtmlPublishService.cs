using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.feature.webhtmlpublish.Interfaces
{
	public interface IWebHtmlPublishService
	{
		Task<List<Tuple<string, bool>>> RenderCopy(List<FileIndexItem> fileIndexItemsList,
			string publishProfileName, string itemName, string outputParentFullFilePathFolder,
			bool moveSourceFiles = false);

		Task GenerateZip(string fullFileParentFolderPath, string itemName,
			IEnumerable<Tuple<string, bool>> renderCopyResult,
			bool deleteFolderAfterwards = false);
	}
}
