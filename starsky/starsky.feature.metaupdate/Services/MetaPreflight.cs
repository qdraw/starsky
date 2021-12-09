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
			Preflight(FileIndexItem inputModel, string[] inputFilePaths,
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
				if ( _iStorage.IsFolderOrFile(fileIndexItem.FilePath) == 
				     FolderOrFileModel.FolderOrFileTypeList.Deleted )
				{
					new StatusCodesHelper().ReturnExifStatusError(fileIndexItem, 
						FileIndexItem.ExifStatus.NotFoundSourceMissing,
						fileIndexUpdateList);
					continue; 
				}
				
				// Dir is readonly / don't edit
				if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(fileIndexItem) 
				     == FileIndexItem.ExifStatus.ReadOnly)
				{
					new StatusCodesHelper().ReturnExifStatusError(fileIndexItem, 
						FileIndexItem.ExifStatus.ReadOnly,
						fileIndexUpdateList);
					continue; 
				}
				
				CompareAllLabelsAndRotation(changedFileIndexItemName,
					fileIndexItem, inputModel, append, rotateClock);
						
				// this one is good :)
				if ( fileIndexItem.Status == FileIndexItem.ExifStatus.Default || fileIndexItem.Status == FileIndexItem.ExifStatus.OkAndSame)
				{
					fileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
				}
				
				// Deleted is allowed but the status need be updated
				if (( new StatusCodesHelper(_appSettings).IsDeletedStatus(fileIndexItem) 
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
		/// Compare Rotation and All other tags
		/// </summary>
		/// <param name="changedFileIndexItemName">Per file stored  string{FilePath},
		/// List*string*{FileIndexItem.name (e.g. Tags) that are changed}</param>
		/// <param name="collectionsFileIndexItem">DetailView input, only to display changes</param>
		/// <param name="statusModel">object that include the changes</param>
		/// <param name="append">true= for tags to add</param>
		/// <param name="rotateClock">rotation value 1 left, -1 right, 0 nothing</param>
		public void CompareAllLabelsAndRotation( Dictionary<string, List<string>> changedFileIndexItemName, 
			FileIndexItem collectionsFileIndexItem, FileIndexItem statusModel, bool append, int rotateClock)
		{
			if ( changedFileIndexItemName == null )
				throw new MissingFieldException(nameof(changedFileIndexItemName));
			
			// compare and add changes to collectionsDetailView
			var comparedNamesList = FileIndexCompareHelper
				.Compare(collectionsFileIndexItem, statusModel, append);
					
			// if requested, add changes to rotation
			collectionsFileIndexItem = 
				RotationCompare(rotateClock, collectionsFileIndexItem, comparedNamesList);

			if ( ! changedFileIndexItemName.ContainsKey(collectionsFileIndexItem.FilePath) )
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
		public FileIndexItem RotationCompare(int rotateClock, FileIndexItem fileIndexItem, 
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
