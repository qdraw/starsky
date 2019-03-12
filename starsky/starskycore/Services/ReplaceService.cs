
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskycore.Services
{
	public class ReplaceService
	{
		private readonly IQuery _query;
		private readonly AppSettings _appSettings;
		
		/// <summary>Do a sync of files uning a subpath</summary>
		/// <param name="query">Starsky IQuery interface to do calls on the database</param>
		/// <param name="appSettings">Settings of the application</param>
		public ReplaceService(IQuery query, AppSettings appSettings)
		{
			_query = query;
			_appSettings = appSettings;
		}

		/// <summary>
		/// Search and replace in string based fields
		/// </summary>
		/// <param name="f">subPath</param>
		/// <param name="search"></param>
		/// <param name="replace"></param>
		/// <param name="fieldName"></param>
		/// <param name="collections"></param>
		public List<FileIndexItem> Replace(string f, string fieldName, string search, string replace, bool collections)
		{
			if ( ! FileIndexCompareHelper.CheckIfPropertyExist(fieldName) ) return new List<FileIndexItem>{new FileIndexItem{Status = FileIndexItem.ExifStatus.OperationNotSupported}};
			var inputFilePaths = PathHelper.SplitInputFilePaths(f);
			
			// the result list
			var fileIndexResultsList = new List<FileIndexItem>();

			// to collect
			foreach ( var subPath in inputFilePaths )
			{
				var detailView = _query.SingleItem(subPath, null, collections, false);
				var statusResults =
					new StatusCodesHelper(_appSettings).FileCollectionsCheck(detailView);

				// To Inject if detailview is false
				var statusModel = new FileIndexItem();
				statusModel.SetFilePath(subPath);
				statusModel.IsDirectory = false;

				// if one item fails, the status will added
				if ( new StatusCodesHelper(null).ReturnExifStatusError(statusModel, statusResults,
					fileIndexResultsList) ) continue;
				if ( detailView == null ) throw new ArgumentNullException(nameof(detailView));
				
				// Now Add Collection based images
				var collectionSubPathList = detailView.GetCollectionSubPathList(detailView, collections, subPath);
				foreach ( var item in collectionSubPathList )
				{
					var itemDetailView = _query.SingleItem(item, null, false, false).FileIndexItem;
					itemDetailView.Status = FileIndexItem.ExifStatus.Ok;
					fileIndexResultsList.Add(itemDetailView);
				}

			}

			foreach ( var fileIndexItem in fileIndexResultsList.Where( p => p.Status == FileIndexItem.ExifStatus.Ok) )
			{
//				var statusModel = new FileIndexItem();
//					
//				FileIndexCompareHelper.SetCompare(null, )
//				FileIndexCompareHelper.Compare(fileIndexItem, statusModel, append);
//				
//				
//				fileIndexItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
//					.Where(p => p.Name == fieldName);
//				
//				Compare
			}

			return fileIndexResultsList;
		}


	}
}
