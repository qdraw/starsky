using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using starsky.feature.metaupdate.Interfaces;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.feature.metaupdate.Services
{
	[Service(typeof(IMetaReplaceService), InjectionLifetime = InjectionLifetime.Scoped)]
	public class MetaReplaceService : IMetaReplaceService
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		private readonly IStorage _iStorage;

		/// <summary>Do a sync of files uning a subpath</summary>
		/// <param name="query">Starsky IQuery interface to do calls on the database</param>
		/// <param name="appSettings">Settings of the application</param>
		/// <param name="selectorStorage">storage abstraction</param>
		public MetaReplaceService(IQuery query, AppSettings appSettings, ISelectorStorage selectorStorage)
		{
			_query = query;
			_appSettings = appSettings;
			_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		}

		/// <summary>
		/// Search and replace in string based fields (only Getting and replacing)
		/// </summary>
		/// <param name="f">subPath (split by dot comma ;)</param>
		/// <param name="search"></param>
		/// <param name="replace"></param>
		/// <param name="fieldName"></param>
		/// <param name="collections"></param>
		public List<FileIndexItem> Replace(string f, string fieldName, string search, string replace, bool collections)
		{
			// when you search for nothing, your fast done
			if ( string.IsNullOrEmpty(search) ) return new List<FileIndexItem>{new FileIndexItem{Status = FileIndexItem.ExifStatus.OperationNotSupported}};

			// escaping null values
			if ( string.IsNullOrEmpty(replace) ) replace = string.Empty;

			if ( ! FileIndexCompareHelper.CheckIfPropertyExist(fieldName) ) return new List<FileIndexItem>{new FileIndexItem{Status = FileIndexItem.ExifStatus.OperationNotSupported}};
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);

			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();

			// to collect
			foreach ( var subPath in inputFilePaths )
			{
				var detailView = _query.SingleItem(subPath, null, collections, false);
				
				// todo: not found??
				
				if ( detailView?.FileIndexItem == null )
				{
					new StatusCodesHelper().ReturnExifStatusError(new FileIndexItem(subPath), 
						FileIndexItem.ExifStatus.NotFoundNotInIndex,
						fileIndexResultsList);
					continue;
				}
				
				// Dir is readonly / don't edit
				if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(detailView) 
				     == FileIndexItem.ExifStatus.ReadOnly)
				{
					new StatusCodesHelper().ReturnExifStatusError(detailView.FileIndexItem, 
						FileIndexItem.ExifStatus.ReadOnly,
						fileIndexResultsList);
					continue; 
				}

				// current item is also ok
				if ( detailView.FileIndexItem.Status == FileIndexItem.ExifStatus.Default )
				{
					detailView.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
				}
				
				// Now Add Collection based images
				var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
				foreach ( var item in collectionSubPathList )
				{
					var itemDetailView = _query.SingleItem(item, null, false, false).FileIndexItem;
					itemDetailView.Status = FileIndexItem.ExifStatus.Ok;
					fileIndexResultsList.Add(itemDetailView);
				}

			}

			fileIndexResultsList = SearchAndReplace(fileIndexResultsList, fieldName, search, replace);

			return fileIndexResultsList;
		}

		public List<FileIndexItem> SearchAndReplace(List<FileIndexItem> fileIndexResultsList, string fieldName, string search, string replace)
		{
			foreach ( var fileIndexItem in fileIndexResultsList.Where( 
				p => p.Status == FileIndexItem.ExifStatus.Ok 
				     || p.Status == FileIndexItem.ExifStatus.Deleted) )
			{
				var searchInObject = FileIndexCompareHelper.Get(fileIndexItem, fieldName);
				var replacedToObject = new object();
				
				PropertyInfo[] propertiesA = new FileIndexItem().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
				PropertyInfo property = propertiesA.FirstOrDefault(p => string.Equals(p.Name, fieldName, StringComparison.InvariantCultureIgnoreCase));

				if ( property.PropertyType == typeof(string) )
				{
					var searchIn = ( string ) searchInObject;
					
					// Replace Ignore Case
					replacedToObject = Regex.Replace(
						searchIn,
						Regex.Escape(search), 
						replace.Replace("$","$$"), 
						RegexOptions.IgnoreCase
					);
				}

				// only string types are added here, other types are ignored for now
				FileIndexCompareHelper.Set(fileIndexItem, fieldName, replacedToObject);
			}

			return fileIndexResultsList;
		}


	}
}
