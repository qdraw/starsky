using System.ComponentModel;
using ModelContextProtocol.Server;
using starsky.project.mcp.Models;
using starsky.feature.metaupdate.Interfaces;
using starsky.feature.search.Interfaces;
using starsky.foundation.database.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.project.mcp.Tools;

/// <summary>
///     Read-only tools: they never change files or the database
/// </summary>
[McpServerToolType]
public sealed class StarskyReadTools(
	ISearch search,
	ISearchSuggest searchSuggest,
	IMetaInfo metaInfo,
	IQuery indexQuery,
	AppSettings appSettings)
{
	private const int MaxFolderItems = 100;

	[McpServerTool(Name = "search_photos", ReadOnly = true, Idempotent = true, OpenWorld = false)]
	[Description("Search the photo library. Query examples: `-Tags:holiday`, " +
	             "`-DateTime>2024-01-01`, `-FilePath:/2024/`, or plain words. " +
	             "Returns one page (zero-based) of results.")]
	public async Task<McpSearchResult> SearchPhotos(
		[Description("search query")] string query,
		[Description("zero-based page number")]
		int page = 0,
		[Description("combine files with the same name before the extension")]
		bool collections = true)
	{
		var result = await search.Search(query, Math.Max(0, page), true, collections);
		return new McpSearchResult(result.SearchCount, result.PageNumber, result.LastPageNumber,
			McpPhoto.FromItems(result.FileIndexItems ?? []));
	}

	[McpServerTool(Name = "list_folder", ReadOnly = true, Idempotent = true, OpenWorld = false)]
	[Description("List the files and subfolders of a folder in the library, " +
	             "not including deleted files. Use `/` for the root.")]
	public McpFolderResult ListFolder(
		[Description("subPath of the folder, for example `/2024/trip`")]
		string path = "/")
	{
		var subPath = McpPaths.ToSubPath(path);
		var items = indexQuery.DisplayFileFolders(subPath).ToList();
		return new McpFolderResult(subPath, items.Count,
			items.Count > MaxFolderItems,
			McpPhoto.FromItems(items, MaxFolderItems),
			appSettings.IsReadOnly(subPath));
	}

	[McpServerTool(Name = "get_photo_info", ReadOnly = true, Idempotent = true,
		OpenWorld = false)]
	[Description("Metadata of one or more files: tags, title, description, color class, " +
	             "date, location. Status is `Ok` when the file exists.")]
	public async Task<IReadOnlyList<McpPhoto>> GetPhotoInfo(
		[Description("subPaths of the files, for example `/2024/trip/img_01.jpg`")]
		string[] paths,
		[Description("include files with the same name before the extension")]
		bool collections = false)
	{
		var subPaths = ( paths ?? [] ).Select(McpPaths.ToSubPath).Distinct()
			.Where(p => p != "/").Take(Math.Max(1, appSettings.McpMaxItemsPerCall)).ToList();
		if ( subPaths.Count == 0 )
		{
			return [];
		}

		var items = await metaInfo.GetInfoAsync(subPaths, collections);
		return McpPhoto.FromItems(items);
	}

	[McpServerTool(Name = "suggest_tags", ReadOnly = true, Idempotent = true, OpenWorld = false)]
	[Description("Search suggestions (tags and keywords in use) that start with the given text.")]
	public async Task<IReadOnlyList<string>> SuggestTags(
		[Description("the beginning of a tag or keyword")]
		string prefix)
	{
		if ( string.IsNullOrWhiteSpace(prefix) )
		{
			return [];
		}

		return ( await searchSuggest.SearchSuggest(prefix, true) ).Take(50).ToList();
	}
}

public sealed record McpSearchResult(
	int TotalCount,
	int Page,
	int LastPage,
	IReadOnlyList<McpPhoto> Items);

public sealed record McpFolderResult(
	string Path,
	int TotalCount,
	bool Truncated,
	IReadOnlyList<McpPhoto> Items,
	bool IsReadOnly);
