using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public sealed class DatabaseThumbnailGenerationJobHandlerTest
{
	[TestMethod]
	public void JobType_ShouldMatchExpected()
	{
		var service = new FakeIDatabaseThumbnailGenerationService();
		var handler = new DatabaseThumbnailGenerationJobHandler(service);
		Assert.AreEqual(DatabaseThumbnailGenerationService.DatabaseThumbnailGenerationJobType, handler.JobType);
	}

	[TestMethod]
	public async Task ExecuteAsync_InvalidJson_ShouldThrow()
	{
		var service = new FakeIDatabaseThumbnailGenerationService();
		var handler = new DatabaseThumbnailGenerationJobHandler(service);
		await Assert.ThrowsExactlyAsync<JsonException>(() =>
			handler.ExecuteAsync("{ invalid }", CancellationToken.None));
	}
}
