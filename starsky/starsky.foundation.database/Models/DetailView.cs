using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using starsky.foundation.platform.Helpers;

namespace starsky.foundation.database.Models;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
[SuppressMessage("ReSharper", "CA1822: static property")]
[SuppressMessage("ReSharper", "S2325: Static property")]
public sealed class DetailView
{
	/// <summary>
	///     Private field for next/prev
	/// </summary>
	private RelativeObjects _relativeObjects = new();

	public FileIndexItem? FileIndexItem { get; set; }

	public List<string> Breadcrumb { get; set; } = new();

	/// <summary>
	///     List of selected Color Class's
	/// </summary>
	public List<ColorClassParser.Color> ColorClassActiveList { get; set; } = new();

	/// <summary>
	///     Used by react client
	/// </summary>
	public string PageType => PageViewType.PageType.DetailView.ToString();

	/// <summary>
	///     To return error codes in the json it is always false
	/// </summary>
	public bool IsDirectory { get; set; }

	/// <summary>
	///     Location of the path
	/// </summary>
	public string SubPath { get; set; } = string.Empty;

	/// <summary>
	///     Is collections enabled?
	/// </summary>
	public bool Collections { get; set; }

	public RelativeObjects RelativeObjects
	{
		get => _relativeObjects;
		set
		{
			_relativeObjects = new RelativeObjects(Collections, ColorClassActiveList);
			_relativeObjects = value;
		}
	}

	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public bool IsReadOnly { get; set; }

	/// <summary>
	///     If collections is enabled return list of subPaths
	///     Does NOT Fill the collection list
	/// </summary>
	/// <param name="fileIndexItem">the base fileIndexItem</param>
	/// <param name="collections">bool, to enable</param>
	/// <param name="subPath">the file original requested in subPath style</param>
	/// <returns></returns>
	public static List<string> GetCollectionSubPathList(FileIndexItem fileIndexItem,
		bool collections, string subPath)
	{
		// Paths that are used
		var collectionSubPathList = fileIndexItem.CollectionPaths;
		// when not running in collections mode only update one file
		if ( !collections )
		{
			collectionSubPathList = new List<string> { subPath };
		}

		return collectionSubPathList;
	}
}
