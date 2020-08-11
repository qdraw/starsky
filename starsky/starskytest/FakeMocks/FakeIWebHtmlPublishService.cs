using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.feature.webhtmlpublish.Interfaces;
using starsky.foundation.database.Models;
#pragma warning disable 1998

namespace starskytest.FakeMocks
{
	public class FakeIWebHtmlPublishService : IWebHtmlPublishService
	{
		public async Task<Dictionary<string, bool>> RenderCopy(List<FileIndexItem> fileIndexItemsList, string publishProfileName, string itemName,
			string outputParentFullFilePathFolder, bool moveSourceFiles = false)
		{
			return new Dictionary<string, bool>();
		}

		public async Task GenerateZip(string fullFileParentFolderPath, string itemName, Dictionary<string, bool> renderCopyResult,
			bool deleteFolderAfterwards = false)
		{
		}
	}
}
