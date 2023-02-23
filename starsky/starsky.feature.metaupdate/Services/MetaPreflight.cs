using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.metaupdate.Helpers;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;

namespace starsky.feature.metaupdate.Services
{
	[Service(typeof(IMetaPreflight), InjectionLifetime = InjectionLifetime.Scoped)]
	public class MetaPreflight : IMetaPreflight
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IWebLogger _logger;

		public MetaPreflight(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage, IWebLogger logger)
		{
			_query = query;
			_appSettings = appSettings;
			_logger = logger;
			if ( selectorStorage != null ) _iStorage = selectorStorage.Get(
				SelectorStorage.StorageServices.SubPath);
		}

		public async Task<(List<FileIndexItem> fileIndexResultsList,
				Dictionary<string, List<string>> changedFileIndexItemName)>
			PreflightAsync(FileIndexItem inputModel, string[] inputFilePaths,
				bool append, bool collections, int rotateClock)
		{
			// the result list
			var fileIndexUpdateList = new List<FileIndexItem>();
			
			// Per file stored key = string[fileHash] item => List <string>
			// FileIndexItem.name (e.g. Tags) that are changed
			var changedFileIndexItemName = new Dictionary<string, List<string>>();
			
			// Prefill cache to avoid fast updating issues
			await new AddParentCacheIfNotExist(_query,_logger).AddParentCacheIfNotExistAsync(inputFilePaths);
			
			var resultFileIndexItemsList = await _query.GetObjectsByFilePathAsync(
				inputFilePaths.ToList(), collections);

			foreach ( var fileIndexItem in resultFileIndexItemsList )
			{
				// Files that are not on disk
				if ( _iStorage.IsFolderOrFile(fileIndexItem.FilePath!) == 
				     FolderOrFileModel.FolderOrFileTypeList.Deleted )
				{
					StatusCodesHelper.ReturnExifStatusError(fileIndexItem, 
						FileIndexItem.ExifStatus.NotFoundSourceMissing,
						fileIndexUpdateList);
					continue; 
				}
				
				// Dir is readonly / don't edit
				if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(fileIndexItem) 
				     == FileIndexItem.ExifStatus.ReadOnly)
				{
					StatusCodesHelper.ReturnExifStatusError(fileIndexItem, 
						FileIndexItem.ExifStatus.ReadOnly,
						fileIndexUpdateList);
					continue; 
				}

				CompareAllLabelsAndRotation(changedFileIndexItemName,
					fileIndexItem, inputModel, append, rotateClock);

				// Add to update list
				CheckGeoLocationStatus(fileIndexItem);

				// this one is good :)
				if ( fileIndexItem.Status is FileIndexItem.ExifStatus.Default or FileIndexItem.ExifStatus.OkAndSame)
				{
					fileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
				}
				
				// Deleted is allowed but the status need be updated
				if (( StatusCodesHelper.IsDeletedStatus(fileIndexItem) 
				      == FileIndexItem.ExifStatus.Deleted) )
				{
					fileIndexItem.Status = FileIndexItem.ExifStatus.Deleted;
				}
				
				// The hash in FileIndexItem is not correct
				// Clone to not change after update
				fileIndexUpdateList.Add(fileIndexItem);
			}

			// update database cache and cloned due reference
			_query.CacheUpdateItem(fileIndexUpdateList);

			AddNotFoundInIndexStatus.Update(inputFilePaths, fileIndexUpdateList);

			return (fileIndexUpdateList, changedFileIndexItemName);
		}

		/// <summary>
		/// Check if the GeoLocation is valid
		/// If not set the status to OperationNotSupported
		/// </summary>
		/// <param name="fileIndexItem">item</param>
		private static void CheckGeoLocationStatus(FileIndexItem fileIndexItem)
		{
			if ( fileIndexItem.Latitude == 0 || fileIndexItem.Longitude == 0 )
				return;
			
			var result = ValidateLocation.ValidateLatitudeLongitude(
				fileIndexItem.Latitude, fileIndexItem.Longitude);
			if ( !result )
			{
				fileIndexItem.Status = FileIndexItem.ExifStatus
					.OperationNotSupported;
			}
		}

		/// <summary>
		/// Compare Rotation and All other tags
		/// </summary>
		/// <param name="changedFileIndexItemName">Per file stored  string{FilePath},
		/// List*string*{FileIndexItem.name (e.g. Tags) that are changed}</param>
		/// <param name="collectionsFileIndexItem">DetailView input, only to display changes</param>
		/// <param name="statusModel">object that include the changes</param>
		/// <param name="append">true= for tags to add</param>
		/// <param name="rotateClock">rotation value 1 left, -1 right, 0 nothing</param>
		public static void CompareAllLabelsAndRotation( Dictionary<string, List<string>> changedFileIndexItemName, 
			FileIndexItem collectionsFileIndexItem, FileIndexItem statusModel, bool append, int rotateClock)
		{
			if ( changedFileIndexItemName == null || string.IsNullOrEmpty(collectionsFileIndexItem.FilePath) )
				throw new MissingFieldException(nameof(changedFileIndexItemName));
			
			// compare and add changes to collectionsDetailView
			var comparedNamesList = FileIndexCompareHelper.Compare(collectionsFileIndexItem, 
				statusModel, append);

			// if requested, add changes to rotation
			collectionsFileIndexItem = 
				RotationCompare(rotateClock, collectionsFileIndexItem, comparedNamesList);

			collectionsFileIndexItem.LastChanged = comparedNamesList;

			if ( ! changedFileIndexItemName.ContainsKey(collectionsFileIndexItem.FilePath!) )
			{
				// add to list
				changedFileIndexItemName.Add(collectionsFileIndexItem.FilePath,comparedNamesList);
				return;
			}

			// overwrite list if already exist
			changedFileIndexItemName[collectionsFileIndexItem.FilePath] = comparedNamesList;
		}
		
		/// <summary>
		/// Add to comparedNames list and add to detail view
		/// </summary>
		/// <param name="rotateClock">-1 or 1</param>
		/// <param name="fileIndexItem">main db object</param>
		/// <param name="comparedNamesList">list of types that are changes</param>
		/// <returns>updated image</returns>
		public static FileIndexItem RotationCompare(int rotateClock, FileIndexItem fileIndexItem, 
			ICollection<string> comparedNamesList)
		{
			// Do orientation / Rotate if needed (after compare)
			if (!FileIndexItem.IsRelativeOrientation(rotateClock)) return fileIndexItem;
			// run this on detail view => statusModel is always default
			fileIndexItem.SetRelativeOrientation(rotateClock);
			
			// list of exifTool to update this field
			if ( !comparedNamesList.Contains(nameof(fileIndexItem.Orientation).ToLowerInvariant()) )
			{
				comparedNamesList.Add(nameof(fileIndexItem.Orientation).ToLowerInvariant());
			}
			return fileIndexItem;
		}
	}
}
