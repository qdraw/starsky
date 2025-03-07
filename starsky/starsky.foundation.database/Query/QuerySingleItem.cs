using System.Collections.Generic;
using System.Linq;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

/// <summary>
///     QuerySingleItem
/// </summary>
public partial class Query
{
	// For displaying single photo's
	// Display feature only?!
	// input: Name of item by db style path
	// With Caching feature :)

	/// <summary>
	///     SingleItemPath do the query for singleItem + return detailView object
	/// </summary>
	/// <param name="singleItemDbPath"></param>
	/// <param name="colorClassActiveList">list of colorClasses to show, default show all</param>
	/// <param name="enableCollections">enable collections feature > default true</param>
	/// <param name="hideDeleted">do not show deleted files > default true</param>
	/// <param name="sort">how to sort</param>
	/// <returns>view object to show on the page (if not exists its null) </returns>
	public DetailView? SingleItem(
		string singleItemDbPath,
		List<ColorClassParser.Color>? colorClassActiveList = null,
		bool enableCollections = true,
		bool hideDeleted = true,
		SortType? sort = SortType.FileName)
	{
		if ( string.IsNullOrWhiteSpace(singleItemDbPath) )
		{
			return null;
		}

		var parentFolder = FilenamesHelper.GetParentPath(singleItemDbPath);
		var fileIndexItemsList = DisplayFileFolders(
			parentFolder, null, false, false).ToList();

		return SingleItem(
			fileIndexItemsList,
			singleItemDbPath,
			colorClassActiveList,
			enableCollections,
			hideDeleted,
			sort);
	}

	/// <summary>
	///     fileIndexItemsList, Create an detailView object
	/// </summary>
	/// <param name="fileIndexItemsList">list of fileIndexItems</param>
	/// <param name="singleItemDbPath">database style path</param>
	/// <param name="colorClassActiveList">list of colorClasses to show, default show all</param>
	/// <param name="enableCollections">enable collections feature > default true</param>
	/// <param name="hideDeleted">do not show deleted files > default true</param>
	/// <param name="sort">how to sort</param>
	/// <returns>view object to show on the page (if not exists its null) </returns>
	public DetailView? SingleItem(
		List<FileIndexItem> fileIndexItemsList,
		string singleItemDbPath,
		List<ColorClassParser.Color>? colorClassActiveList = null,
		bool enableCollections = true,
		bool hideDeleted = true,
		SortType? sort = SortType.FileName)
	{
		// reject empty requests
		if ( string.IsNullOrWhiteSpace(singleItemDbPath) )
		{
			return null;
		}

		var parentFolder = FilenamesHelper.GetParentPath(singleItemDbPath);

		// RemoveLatestSlash is for '/' folder
		var fileName = singleItemDbPath.Replace(
			PathHelper.RemoveLatestSlash(parentFolder) + "/", string.Empty);

		// Home has no parent, so return a value 
		var objectByFilePath = GetObjectByFilePath("/");
		if ( fileName == string.Empty && parentFolder == "/" && objectByFilePath != null )
		{
			// This is for HOME only
			return new DetailView
			{
				FileIndexItem = objectByFilePath,
				RelativeObjects = new RelativeObjects(),
				Breadcrumb = new List<string> { "/" },
				ColorClassActiveList =
					colorClassActiveList ?? new List<ColorClassParser.Color>(),
				IsDirectory = true,
				SubPath = "/",
				Collections = enableCollections
			};
		}

		var currentFileIndexItem = fileIndexItemsList.Find(p => p.FileName == fileName);

		// Could be not found or not in directory cache
		if ( currentFileIndexItem == null )
		{
			// retry
			currentFileIndexItem = GetObjectByFilePath(singleItemDbPath);
			if ( currentFileIndexItem == null )
			{
				return null;
			}

			AddCacheItem(currentFileIndexItem);
		}

		// To know when a file is deleted
		if ( currentFileIndexItem.Tags != null &&
		     currentFileIndexItem.Tags.Contains(TrashKeyword.TrashKeywordString) )
		{
			currentFileIndexItem.Status = FileIndexItem.ExifStatus.Deleted;
		}

		if ( currentFileIndexItem.IsDirectory == true )
		{
			currentFileIndexItem.CollectionPaths = new List<string> { singleItemDbPath };

			return new DetailView
			{
				IsDirectory = true,
				SubPath = singleItemDbPath,
				FileIndexItem = currentFileIndexItem,
				Collections = enableCollections
			};
		}

		if ( currentFileIndexItem.Tags != null &&
		     currentFileIndexItem.Tags.Contains(TrashKeyword.TrashKeywordString) )
		{
			hideDeleted = false;
		}

		var fileIndexItemsForPrevNextList = DisplayFileFolders(
			parentFolder, colorClassActiveList, enableCollections, hideDeleted).ToList();

		var itemResult = new DetailView
		{
			FileIndexItem = currentFileIndexItem,
			RelativeObjects =
				GetNextPrevInSubFolder(currentFileIndexItem, fileIndexItemsForPrevNextList,
					sort ?? SortType.FileName),
			Breadcrumb = Breadcrumbs.BreadcrumbHelper(singleItemDbPath),
			ColorClassActiveList =
				colorClassActiveList ?? new List<ColorClassParser.Color>(),
			IsDirectory = false,
			SubPath = singleItemDbPath,
			Collections = enableCollections
		};

		// First item is current item
		var collectionPaths = new List<string> { singleItemDbPath };
		// include directories here
		collectionPaths.AddRange(fileIndexItemsList
			.Where(p => p.FileCollectionName == currentFileIndexItem.FileCollectionName)
			.Select(p => p.FilePath)!);

		var collectionPathsHashSet = new HashSet<string>(collectionPaths);
		itemResult.FileIndexItem.CollectionPaths = collectionPathsHashSet.ToList();

		return itemResult;
	}

	private static RelativeObjects GetNextPrevInSubFolder(
		FileIndexItem? currentFileIndexItem,
		List<FileIndexItem> fileIndexItemsList, SortType sortType)
	{
		// Check if this is item is not !deleted! yet
		if ( currentFileIndexItem == null )
		{
			return new RelativeObjects();
		}

		fileIndexItemsList = SortHelper.Helper(fileIndexItemsList, sortType).ToList();

		var currentIndex = fileIndexItemsList.FindIndex(p =>
			p.FilePath == currentFileIndexItem.FilePath);
		var relativeObject = new RelativeObjects();

		if ( currentIndex != fileIndexItemsList.Count - 1 )
		{
			relativeObject.NextFilePath = fileIndexItemsList[currentIndex + 1].FilePath!;
			relativeObject.NextHash = fileIndexItemsList[currentIndex + 1].FileHash!;
		}

		if ( currentIndex >= 1 )
		{
			relativeObject.PrevFilePath = fileIndexItemsList[currentIndex - 1].FilePath!;
			relativeObject.PrevHash = fileIndexItemsList[currentIndex - 1].FileHash!;
		}

		return relativeObject;
	}
}
