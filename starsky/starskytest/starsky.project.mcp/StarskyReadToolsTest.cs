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
		Assert.AreEqual(100, result.Items.Count);
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

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("x", result[0].Tags);
	}

	[TestMethod]
	public async Task GetPhotoInfo_RespectsMaxItems()
	{
		var items = Enumerable.Range(0, 5).Select(i => Item($"/{i}.jpg")).ToList();
		var tools = Create(items, appSettings: new AppSettings { McpMaxItemsPerCall = 2 });

		var result = await tools.GetPhotoInfo(items.Select(p => p.FilePath!).ToArray());

		Assert.AreEqual(2, result.Count);
	}

	[TestMethod]
	public async Task GetPhotoInfo_NoPathsOrOnlyRoot_ReturnsEmpty()
	{
		var tools = Create([Item("/a.jpg")]);

		Assert.AreEqual(0, ( await tools.GetPhotoInfo([]) ).Count);
		Assert.AreEqual(0, ( await tools.GetPhotoInfo(["/", ""]) ).Count);
		Assert.AreEqual(0, ( await tools.GetPhotoInfo(null!) ).Count);
	}

	[TestMethod]
	public async Task SuggestTags_ReturnsMatchesAndIgnoresEmpty()
	{
		var tools = Create(suggestions: ["holiday", "home", "work"]);

		var result = await tools.SuggestTags("ho");

		CollectionAssert.AreEqual(new[] { "holiday", "home" }, result.ToArray());
		Assert.AreEqual(0, ( await tools.SuggestTags(" ") ).Count);
	}
}
