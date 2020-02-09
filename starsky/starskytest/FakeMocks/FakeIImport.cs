using System.Collections.Generic;
using System.Linq;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;

namespace starskytest.FakeMocks
{
	public class FakeIImport : IImport
	{
		public List<string> Import(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings)
		{
			return Preflight(inputFullPathList.ToList(), importSettings)
				.Where(p => p.FilePath != null)
				.Select(p=> p.FilePath ).ToList();
		}

		public List<ImportIndexItem> Preflight(List<string> inputFileFullPaths, ImportSettingsModel importSettings)
		
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
					new StorageHostFullPathFilesystem().ReadStream(inputFileFullPath, 160));

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
