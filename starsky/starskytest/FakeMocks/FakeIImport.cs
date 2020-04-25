using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskytest.FakeMocks
{
	public class FakeIImport : IImport
	{
		private ISelectorStorage _selectorStorage;

		public FakeIImport(ISelectorStorage selectorStorage)
		{
			_selectorStorage = selectorStorage;
		}
		
		public List<string> ImportTo(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings)
		{
			return Preflight(inputFullPathList.ToList(), importSettings).Result
				.Where(p => p.FilePath != null)
				.Select(p=> p.FilePath ).ToList();
		}

		public List<string> ImportTo(string inputFullPathList, ImportSettingsModel importSettings)
		{
			throw new System.NotImplementedException();
		}

		public List<string> Import(string inputFullPathList, ImportSettingsModel importSettings)
		{
			throw new System.NotImplementedException();
		}

		public async Task<List<ImportIndexItem>> Preflight(List<string> inputFileFullPaths, ImportSettingsModel importSettings)
		{
			var results = new List<ImportIndexItem>();
			foreach ( var inputFileFullPath in inputFileFullPaths )
			{
				// if the item fails
				var importIndexFileError = new ImportIndexItem {
					FilePath = "/" + FilenamesHelper.GetFileName(inputFileFullPath),
					FileHash = "FAKE", 
					Status = ImportStatus.FileError
				};
			
				// Check if extension is correct
				if (!ExtensionRolesHelper.IsExtensionSyncSupported(inputFileFullPath)) 
				{
					results.Add(importIndexFileError);
				}

				// Check if the file is correct
				var imageFormat = ExtensionRolesHelper.GetImageFormat(
					_selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem)
						.ReadStream(inputFileFullPath, 160));

				if ( ! ExtensionRolesHelper.ExtensionSyncSupportedList.Contains($"{imageFormat}") )
				{
					results.Add(importIndexFileError);
				}
				
				results.Add(new ImportIndexItem
				{
					SourceFullFilePath = inputFileFullPath,
					Status = ImportStatus.Ok,
					FileHash = "FAKE"
				});
			}
			return results;
		}

		public List<ImportIndexItem> History()
		{
			throw new System.NotImplementedException();
		}

	}
}
