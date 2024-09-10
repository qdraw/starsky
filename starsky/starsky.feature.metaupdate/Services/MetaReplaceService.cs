using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
	[Service(typeof(IMetaReplaceService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class MetaReplaceService : IMetaReplaceService
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;
		private readonly IWebLogger _logger;

		/// <summary>Replace meta content</summary>
		/// <param name="query">Starsky IQuery interface to do calls on the database</param>
		/// <param name="appSettings">Settings of the application</param>
		/// <param name="selectorStorage">storage abstraction</param>
		/// <param name="logger">web logger</param>
		public MetaReplaceService(IQuery query, AppSettings appSettings,
			ISelectorStorage? selectorStorage, IWebLogger logger)
		{
			_query = query;
			_appSettings = appSettings;
			_iStorage = new StorageHostFullPathFilesystem(logger);
			if ( selectorStorage != null )
			{
				_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
			}

			_logger = logger;
		}

		/// <summary>
		/// Search and replace in string based fields (only Getting and replacing)
		/// </summary>
		/// <param name="f">subPath (split by dot comma ;)</param>
		/// <param name="fieldName">e.g. tags</param>
		/// <param name="search">from</param>
		/// <param name="replace">to</param>
		/// <param name="collections">stack collections</param>
		public async Task<List<FileIndexItem>> Replace(string f, string fieldName, string search,
			string replace, bool collections)
		{
			// when you search for nothing, your fast done
			if ( string.IsNullOrEmpty(search) )
			{
				return
				[
					new FileIndexItem { Status = FileIndexItem.ExifStatus.OperationNotSupported }
				];
			}

			// escaping null values
			if ( string.IsNullOrEmpty(replace) )
			{
				replace = string.Empty;
			}

			if ( !FileIndexCompareHelper.CheckIfPropertyExist(fieldName) )
			{
				return new List<FileIndexItem>
				{
					new FileIndexItem
					{
						Status = FileIndexItem.ExifStatus.OperationNotSupported
					}
				};
			}

			var inputFilePaths = PathHelper.SplitInputFilePaths(f).ToList();
			inputFilePaths =
				AppendXmpPathsWhenCollectionsFalseHelper.AppendXmpPathsWhenCollectionsFalse(
					collections, inputFilePaths);

			// the result list
			var fileIndexUpdatedList = new List<FileIndexItem>();

			// Prefill cache to avoid fast updating issues
			await new AddParentCacheIfNotExist(_query, _logger).AddParentCacheIfNotExistAsync(
				inputFilePaths);

			// Assumes that this give status Ok back by default
			var queryFileIndexItemsList = await _query.GetObjectsByFilePathAsync(
				inputFilePaths, collections);

			// to collect
			foreach ( var fileIndexItem in queryFileIndexItemsList )
			{
				// if folder is deleted
				if ( _iStorage.IsFolderOrFile(fileIndexItem.FilePath!) ==
					 FolderOrFileModel.FolderOrFileTypeList.Deleted )
				{
					StatusCodesHelper.ReturnExifStatusError(fileIndexItem,
						FileIndexItem.ExifStatus.NotFoundSourceMissing,
						fileIndexUpdatedList);
					continue;
				}

				// Dir is readonly / don't edit
				if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(fileIndexItem)
					 == FileIndexItem.ExifStatus.ReadOnly )
				{
					StatusCodesHelper.ReturnExifStatusError(fileIndexItem,
						FileIndexItem.ExifStatus.ReadOnly,
						fileIndexUpdatedList);
					continue;
				}

				fileIndexUpdatedList.Add(fileIndexItem);
			}

			fileIndexUpdatedList =
				SearchAndReplace(fileIndexUpdatedList, fieldName, search, replace);

			AddNotFoundInIndexStatus.Update(inputFilePaths, fileIndexUpdatedList);

			var fileIndexResultList = new List<FileIndexItem>();
			foreach ( var fileIndexItem in fileIndexUpdatedList )
			{
				// Status Ok is already set

				// Deleted is allowed but the status need be updated
				if ( ( fileIndexItem.Status == FileIndexItem.ExifStatus.Ok ) &&
					 StatusCodesHelper.IsDeletedStatus(fileIndexItem) ==
					 FileIndexItem.ExifStatus.Deleted )
				{
					fileIndexItem.Status = FileIndexItem.ExifStatus.Deleted;
				}

				fileIndexResultList.Add(fileIndexItem);
			}

			return await new Duplicate(_query).RemoveDuplicateAsync(fileIndexResultList);
		}

		public static List<FileIndexItem> SearchAndReplace(List<FileIndexItem> fileIndexResultsList,
			string fieldName, string search, string replace)
		{
			foreach ( var fileIndexItem in fileIndexResultsList.Where(
						 p => p.Status
							 is FileIndexItem.ExifStatus.Ok
							 or FileIndexItem.ExifStatus.OkAndSame
							 or FileIndexItem.ExifStatus.Deleted
							 or FileIndexItem.ExifStatus.DeletedAndSame) )
			{
				var searchInObject = FileIndexCompareHelper.Get(fileIndexItem, fieldName);
				var replacedToObject = new object();

				var propertiesA = new FileIndexItem().GetType().GetProperties(
					BindingFlags.Public | BindingFlags.Instance);

				var property = Array.Find(propertiesA, p => string.Equals(
					p.Name, fieldName,
					StringComparison.InvariantCultureIgnoreCase));

				if ( property?.PropertyType == typeof(string) )
				{
					var searchIn = searchInObject != null ? ( string ) searchInObject : string.Empty;

					// Replace Ignore Case
					replacedToObject = Regex.Replace(
						searchIn,
						Regex.Escape(search),
						replace.Replace("$", "$$"),
						RegexOptions.IgnoreCase,
						TimeSpan.FromMilliseconds(100)
					);
				}

				// only string types are added here, other types are ignored for now
				FileIndexCompareHelper.Set(fileIndexItem, fieldName, replacedToObject);
			}

			return fileIndexResultsList;
		}
	}
}
