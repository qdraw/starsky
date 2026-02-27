using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Helpers;

[TestClass]
public class IsValidZipOrFolderHelperTest
{
	[TestMethod]
	public async Task IsValidZipOrFolder_EmptyPath_ReturnsNull()
	{
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var helper = new IsValidZipOrFolderHelper(storage, logger);

		var result = await helper.IsValidZipOrFolder("");

		Assert.IsNull(result);
		Assert.IsTrue(logger.TrackedExceptions.Exists(p =>
			p.Item2?.Contains("use the -p to add a path") == true));
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_FolderWithManifest_ReturnsManifest()
	{
		var manifestJson = "{\"slug\":\"test-slug\",\"copy\":{\"file.jpg\":true}}";
		var storage = new FakeIStorage(
			["/folder", "/folder/_settings.json"],
			["/folder/_settings.json"],
			[Encoding.UTF8.GetBytes(manifestJson)]);

		var helper = new IsValidZipOrFolderHelper(storage, new FakeIWebLogger());

		var result = await helper.IsValidZipOrFolder("/folder");

		Assert.IsNotNull(result);
		Assert.AreEqual("test-slug", result.Slug);
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_FolderWithoutManifest_ReturnsNull()
	{
		var storage = new FakeIStorage(["/folder"], []);
		var logger = new FakeIWebLogger();
		var helper = new IsValidZipOrFolderHelper(storage, logger);

		var result = await helper.IsValidZipOrFolder("/folder");

		Assert.IsNull(result);
		Assert.IsTrue(logger.TrackedExceptions.Exists(p =>
			p.Item2?.Contains("starskywebhtmlcli") == true));
	}

	[TestMethod]
	public async Task IsValidZipOrFolder_DeletedPath_ReturnsNull()
	{
		var storage = new FakeIStorage();
		var logger = new FakeIWebLogger();
		var helper = new IsValidZipOrFolderHelper(storage, logger);

		var result = await helper.IsValidZipOrFolder("/deleted");

		Assert.IsNull(result);
		Assert.IsTrue(logger.TrackedExceptions.Exists(p =>
			p.Item2?.Contains("is not found") == true));
	}
}
