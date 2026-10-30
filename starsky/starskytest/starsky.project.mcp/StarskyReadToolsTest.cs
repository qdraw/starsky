using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.project.mcp.Tools;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.project.mcp;

[TestClass]
public sealed class StarskyReadToolsTest
{
	private static readonly string[] HolidayAndHome = ["holiday", "home"];

	private static StarskyReadTools Create(List<FileIndexItem>? items = null,
		FakeISearch? search = null, AppSettings? appSettings = null,
		List<string>? suggestions = null)
	{
		items ??= [];
		return new StarskyReadTools(search ?? new FakeISearch(items),
			new FakeISearchSuggest(suggestions), new FakeIMetaInfo(items),
			new FakeIQuery(items), appSettings ?? new AppSettings());
	}

	private static FileIndexItem Item(string path, string tags = "")
	{
		return new FileIndexItem(path)
		{
			Tags = tags, IsDirectory = false, Status = FileIndexItem.ExifStatus.Ok
		};
	}

	[TestMethod]
	public async Task SearchPhotos_ReturnsCompactItems()
	{
		var search = new FakeISearch([Item("/a/one.jpg", "holiday")]);
		var tools = Create(search: search);

		var result = await tools.SearchPhotos("holiday", 2);

		Assert.AreEqual("holiday", search.LastQuery);
		Assert.AreEqual(2, search.LastPage);
		Assert.AreEqual(1, result.TotalCount);
		Assert.AreEqual("/a/one.jpg", result.Items[0].Path);
		Assert.AreEqual("holiday", result.Items[0].Tags);
	}

	[TestMethod]
	public async Task SearchPhotos_NegativePage_UsesFirstPage()
	{
		var search = new FakeISearch();
		var tools = Create(search: search);

		await tools.SearchPhotos("x", -5);

		Assert.AreEqual(0, search.LastPage);
	}

	[TestMethod]
	public void ListFolder_ReturnsFolderContent()
	{
		var tools = Create([Item("/trip/a.jpg"), Item("/trip/b.jpg"), Item("/other/c.jpg")]);

		var result = tools.ListFolder("trip/");

		Assert.AreEqual("/trip", result.Path);
		Assert.AreEqual(2, result.TotalCount);
		Assert.IsFalse(result.Truncated);
		Assert.IsFalse(result.IsReadOnly);
	}

	[TestMethod]
	public void ListFolder_MoreThanLimit_IsTruncated()
	{
		var items = Enumerable.Range(0, 120).Select(i => Item($"/big/{i}.jpg")).ToList();
		var tools = Create(items);

		var result = tools.ListFolder("/big");

		Assert.AreEqual(120, result.TotalCount);
		Assert.IsTrue(result.Truncated);
		Assert.HasCount(100, result.Items);
	}

	[TestMethod]
	public void ListFolder_ReadOnlyFolder_IsMarked()
	{
		var tools = Create([Item("/ro/a.jpg")],
			appSettings: new AppSettings { ReadOnlyFolders = ["/ro"] });

		Assert.IsTrue(tools.ListFolder("/ro").IsReadOnly);
	}

	[TestMethod]
	public async Task GetPhotoInfo_ReturnsExistingItems()
	{
		var tools = Create([Item("/a.jpg", "x")]);

		var result = await tools.GetPhotoInfo(["a.jpg", "/missing.jpg"]);

		Assert.HasCount(1, result);
		Assert.AreEqual("x", result[0].Tags);
	}

	[TestMethod]
	public async Task GetPhotoInfo_RespectsMaxItems()
	{
		var items = Enumerable.Range(0, 5).Select(i => Item($"/{i}.jpg")).ToList();
		var tools = Create(items, appSettings: new AppSettings { McpMaxItemsPerCall = 2 });

		var result = await tools.GetPhotoInfo(items.Select(p => p.FilePath!).ToArray());

		Assert.HasCount(2, result);
	}

	[TestMethod]
	public async Task GetPhotoInfo_NoPathsOrOnlyRoot_ReturnsEmpty()
	{
		var tools = Create([Item("/a.jpg")]);

		Assert.IsEmpty(await tools.GetPhotoInfo([]));
		Assert.IsEmpty(await tools.GetPhotoInfo(["/", ""]));
		Assert.IsEmpty(await tools.GetPhotoInfo(null!));
	}

	[TestMethod]
	public async Task SuggestTags_ReturnsMatchesAndIgnoresEmpty()
	{
		var tools = Create(suggestions: ["holiday", "home", "work"]);

		var result = await tools.SuggestTags("ho");

		Assert.AreSequenceEqual(HolidayAndHome, result);
		Assert.IsEmpty(await tools.SuggestTags(" "));
	}

	[TestMethod]
	public async Task SearchPhotos_NoItemsInResult_ReturnsEmptyList()
	{
		var search = new FakeISearch { ReturnNullItems = true };
		var tools = Create(search: search);

		var result = await tools.SearchPhotos("x");

		Assert.IsEmpty(result.Items);
	}

	[TestMethod]
	public void ListFolder_NullOrRoot_UsesRoot()
	{
		var tools = Create([Item("/a.jpg")]);

		Assert.AreEqual("/", tools.ListFolder(null!).Path);
		Assert.AreEqual("/", tools.ListFolder("/").Path);
		Assert.AreEqual("/", tools.ListFolder("").Path);
	}
}
