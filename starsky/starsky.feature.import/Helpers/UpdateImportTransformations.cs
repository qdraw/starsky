#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.feature.import.Helpers
{
	public class UpdateImportTransformations
	{
		private readonly IWebLogger _logger;
		private readonly IStorage _subPathStorage;
		private readonly IStorage _thumbnailStorage;
		private readonly IExifTool _exifTool;
		private readonly AppSettings _appSettings;
		private readonly IThumbnailQuery _thumbnailQuery;

		public UpdateImportTransformations(IWebLogger logger, 
			IExifTool exifTool, ISelectorStorage selectorStorage, AppSettings appSettings, IThumbnailQuery thumbnailQuery)
		{
			_logger = logger;
			_exifTool = exifTool;
			_subPathStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			_thumbnailStorage = selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail);
			_appSettings = appSettings;
			_thumbnailQuery = thumbnailQuery;
		}

		
		public delegate Task<FileIndexItem> QueryUpdateDelegate(FileIndexItem fileIndexItem);
		public delegate Task<List<ThumbnailItem>?> QueryThumbnailUpdateDelegate(List<ThumbnailResultDataTransferModel> thumbnailItems);

		/// <summary>
		/// Run Transformation on Import to the files in the database && Update fileHash in database
		/// </summary>
		/// <param name="queryUpdateDelegate"></param>
		/// <param name="fileIndexItem">information</param>
		/// <param name="colorClassTransformation">change colorClass</param>
		/// <param name="dateTimeParsedFromFileName">is date time parsed from fileName</param>
		/// <param name="indexMode">should update database</param>
		internal async Task<FileIndexItem> UpdateTransformations(
			QueryUpdateDelegate? queryUpdateDelegate,
			FileIndexItem fileIndexItem,
			int colorClassTransformation, bool dateTimeParsedFromFileName,
			bool indexMode)
		{
			if ( !ExtensionRolesHelper.IsExtensionExifToolSupported(fileIndexItem.FileName) ) return fileIndexItem;

			var comparedNamesList = new List<string>();
			if ( dateTimeParsedFromFileName )
			{
				_logger.LogInformation($"[Import] DateTimeParsedFromFileName ExifTool Sync {fileIndexItem.FilePath}");
				comparedNamesList = DateTimeParsedComparedNamesList();
			}

			if ( colorClassTransformation >= 0 )
			{
				_logger.LogInformation($"[Import] ColorClassComparedNamesList ExifTool Sync {fileIndexItem.FilePath}");
				comparedNamesList = ColorClassComparedNamesList(comparedNamesList);
			}

			if ( !comparedNamesList.Any() ) return fileIndexItem;

			var exifToolCmdHelper = new ExifToolCmdHelper(_exifTool,
				_subPathStorage, _thumbnailStorage,
				new ReadMeta(_subPathStorage, _appSettings, null, _logger),
				_thumbnailQuery);
			await exifToolCmdHelper.UpdateAsync(fileIndexItem, comparedNamesList);

			// Only update database when indexMode is true
			if ( !indexMode || queryUpdateDelegate == null) return fileIndexItem;
			
			// Hash is changed after transformation
			fileIndexItem.FileHash = (await new FileHash(_subPathStorage).GetHashCodeAsync(fileIndexItem.FilePath!)).Key;

			await queryUpdateDelegate(fileIndexItem);

			return fileIndexItem.Clone();
		}
		
		internal static List<string> DateTimeParsedComparedNamesList()
		{
			return new List<string>
			{
				nameof(FileIndexItem.Description).ToLowerInvariant(),
				nameof(FileIndexItem.DateTime).ToLowerInvariant(),
			};
		}
		
		internal static List<string> ColorClassComparedNamesList(List<string> list)
		{
			list.Add(nameof(FileIndexItem.ColorClass).ToLowerInvariant());
			return list;
		}

	}
}

