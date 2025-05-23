using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Query;

public partial class Query // For folder displays only
{
	// Class for displaying folder content
	// This is the query part
	public IEnumerable<FileIndexItem> DisplayFileFolders(
		string subPath = "/",
		List<ColorClassParser.Color>? colorClassActiveList = null,
		bool enableCollections = true,
		bool hideDeleted = true)
	{
		if ( subPath != "/" )
		{
			PathHelper.RemoveLatestSlash(subPath);
		}

		var fileIndexItems = CacheQueryDisplayFileFolders(subPath);

		return DisplayFileFolders(fileIndexItems,
			colorClassActiveList,
			enableCollections,
			hideDeleted);
	}

	// Display File folder displays content of the folder
	// without any query in this method
	public IEnumerable<FileIndexItem> DisplayFileFolders(
		List<FileIndexItem> fileIndexItems,
		List<ColorClassParser.Color>? colorClassActiveList = null,
		bool enableCollections = true,
		bool hideDeleted = true)
	{
		colorClassActiveList ??= new List<ColorClassParser.Color>();

		// Hide meta files in list
		fileIndexItems = fileIndexItems.Where(p =>
			p.ImageFormat != ExtensionRolesHelper.ImageFormat.xmp &&
			p.ImageFormat != ExtensionRolesHelper.ImageFormat.meta_json).ToList();

		if ( colorClassActiveList.Count != 0 )
		{
			fileIndexItems = fileIndexItems.Where(p => colorClassActiveList.Contains(p.ColorClass))
				.ToList();
		}

		if ( fileIndexItems.Count == 0 )
		{
			return new List<FileIndexItem>();
		}

		if ( enableCollections )
		{
			// Query Collections
			fileIndexItems = StackCollections(fileIndexItems);
		}

		return hideDeleted ? HideDeletedFileFolderList(fileIndexItems) : fileIndexItems;
	}

	public Tuple<bool, List<FileIndexItem>> CacheGetParentFolder(string subPath)
	{
		var fallbackResult = new Tuple<bool, List<FileIndexItem>>(false, []);
		if ( _cache == null || _appSettings.AddMemoryCache == false )
		{
			return fallbackResult;
		}

		// Return values from IMemoryCache
		var queryCacheName = CachingDbName(nameof(FileIndexItem),
			subPath);

		if ( !_cache.TryGetValue(queryCacheName,
			    out var objectFileFolders) )
		{
			return fallbackResult;
		}

		var result = ( objectFileFolders as List<FileIndexItem> ??
		               new List<FileIndexItem>() ).DistinctBy(p => p.FilePath).ToList();
		return new Tuple<bool, List<FileIndexItem>>(true, result);
	}

	/// <summary>
	///     Show previous en next items in the folder view.
	///     There is equivalent class for prev next in the display view
	/// </summary>
	/// <param name="currentFolder">subPath style</param>
	/// <returns>relative object</returns>
	public RelativeObjects GetNextPrevInFolder(string currentFolder)
	{
		if ( currentFolder != "/" )
		{
			PathHelper.RemoveLatestSlash(currentFolder);
		}

		// We use breadcrumbs to get the parent folder
		var parentFolderPath = FilenamesHelper.GetParentPath(currentFolder);

		var itemsInSubFolder = QueryGetNextPrevInFolder(parentFolderPath, currentFolder);

		var photoIndexOfSubFolder = itemsInSubFolder.FindIndex(p => p.FilePath == currentFolder);

		var relativeObject = new RelativeObjects();
		if ( photoIndexOfSubFolder != itemsInSubFolder.Count - 1 && currentFolder != "/" )
		{
			// currentFolder != "/" >= on the home folder you will automatically go to a subfolder
			relativeObject.NextFilePath = itemsInSubFolder[photoIndexOfSubFolder + 1].FilePath!;
			relativeObject.NextHash = itemsInSubFolder[photoIndexOfSubFolder + 1].FileHash!;
		}

		if ( photoIndexOfSubFolder >= 1 )
		{
			relativeObject.PrevFilePath = itemsInSubFolder[photoIndexOfSubFolder - 1].FilePath!;
			relativeObject.PrevHash = itemsInSubFolder[photoIndexOfSubFolder - 1].FileHash!;
		}

		return relativeObject;
	}

	private List<FileIndexItem> CacheQueryDisplayFileFolders(string subPath)
	{
		// The CLI programs uses no cache
		if ( _cache == null || _appSettings.AddMemoryCache == false )
		{
			return QueryDisplayFileFolders(subPath);
		}

		var (isSuccess, objectFileFolders) = CacheGetParentFolder(subPath);

		if ( isSuccess )
		{
			return objectFileFolders;
		}

		objectFileFolders = QueryDisplayFileFolders(subPath);

		AddCacheParentItem(subPath, objectFileFolders);
		return objectFileFolders;
	}

	internal List<FileIndexItem> QueryDisplayFileFolders(string subPath = "/")
	{
		List<FileIndexItem> QueryItems(ApplicationDbContext context)
		{
			var queryItems = context.FileIndex
				.TagWith("QueryDisplayFileFolders")
				.Where(p => p.ParentDirectory == subPath && p.FileName != "/")
				.OrderBy(p => p.FileName).AsEnumerable().DistinctBy(p => p.FileName);
			return queryItems.OrderBy(p => p.FileName, StringComparer.InvariantCulture).ToList();
		}

		try
		{
			return QueryItems(_context);
		}
		catch ( NotSupportedException )
		{
			// System.NotSupportedException:  The ReadAsync method cannot be called when another read operation is pending.
			var context = new InjectServiceScope(_scopeFactory).Context();
			return QueryItems(context);
		}
		catch ( InvalidOperationException ) // or ObjectDisposedException
		{
			var context = new InjectServiceScope(_scopeFactory).Context();
			return QueryItems(context);
		}
	}

	/// <summary>
	///     Hide Deleted items in folder
	/// </summary>
	/// <param name="queryItems">list of items</param>
	/// <returns>list without deleted items</returns>
	private static List<FileIndexItem> HideDeletedFileFolderList(List<FileIndexItem> queryItems)
	{
		// temp feature to hide deleted items
		var displayItems = new List<FileIndexItem>();
		foreach ( var item in queryItems )
		{
			if ( item.Tags != null && !item.Tags.Contains(TrashKeyword.TrashKeywordString) )
			{
				displayItems.Add(item);
			}
		}

		return displayItems;
		// temp feature to hide deleted items
	}
}
