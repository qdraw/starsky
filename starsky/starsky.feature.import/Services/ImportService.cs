using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.import.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Models;

namespace starsky.feature.import.Services
{
	public class ImportService : IImport
	{
		private readonly IImportQuery _importQuery;
		
		// storage providers
		private readonly ISelectorStorage _selectorStorage;
		private readonly IStorage _filesystemStorage;
		private readonly IStorage _subPathStorage;
		private readonly IStorage _thumbnailStorage;
		
		public ImportService(
			ISelectorStorage selectorStorage,
			AppSettings appSettings,
			IImportQuery importQuery,
			IExifTool exifTool,
			IServiceScopeFactory scopeFactory)
		{
			_selectorStorage = selectorStorage;
			_importQuery = importQuery;
			
			_filesystemStorage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
            _subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
            _thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
		}

		public async Task<IEnumerable<FileIndexItem>> Preflight(List<string> fullFilePathsList, ImportSettingsModel importSettings)
		{
			List<Task<FileIndexItem>> listOfTasks = new List<Task<FileIndexItem>>();
				
			foreach ( var fullFilePath in fullFilePathsList )
			{
				listOfTasks.Add(PreflightPerFile(fullFilePath, importSettings));
			}
			return await Task.WhenAll(listOfTasks);
		}

		public List<string> Import(IEnumerable<string> inputFullPathList, ImportSettingsModel importSettings)
		{
			throw new System.NotImplementedException();
		}

		internal async Task<FileIndexItem> PreflightPerFile(string fullFilePath, ImportSettingsModel importSettings)
		{
			if ( _filesystemStorage.ExistFile(fullFilePath) ) return new FileIndexItem(fullFilePath){Status = FileIndexItem.ExifStatus.NotFoundSourceMissing};

			return new FileIndexItem();
			
			// if ( _importQuery.IsHashInImportDb() )
			// {
			// 	
			// }
		}
	}
}
