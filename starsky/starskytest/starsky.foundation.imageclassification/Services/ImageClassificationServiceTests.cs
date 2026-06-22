using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.imageclassification.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.imageclassification.Services;

[TestClass]
public sealed class ImageClassificationServiceTests
{
	public TestContext TestContext { get; set; }

	private sealed class FakeOllamaService : IOllamaService
	{
		public OllamaCommandResult InferResult { get; set; } = new() { Success = true, Output = "cat, tree" };
		public string LastInferPath { get; private set; } = string.Empty;

		public Task<bool> EnsureServeIsRunning(CancellationToken cancellationToken = default)
		{
			return Task.FromResult(true);
		}

		public Task<bool> StopServeAsync(CancellationToken cancellationToken = default)
		{
			return Task.FromResult(true);
		}

		public Task<OllamaCommandResult> GenerateAsync(string prompt,
			CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new OllamaCommandResult { Success = true, Output = prompt });
		}

		public Task<OllamaCommandResult> InferTagsAsync(string imageFilePath,
			CancellationToken cancellationToken = default)
		{
			LastInferPath = imageFilePath;
			return Task.FromResult(InferResult);
		}
	}

	[TestMethod]
	public async Task ClassifyAndUpdateAsync_ShouldMergeAndPersistViaMetaUpdate()
	{
		var fakeOllama = new FakeOllamaService
		{
			InferResult = new OllamaCommandResult { Success = true, Output = "cat, tree, mountain, night" }
		};
		var fakeMetaUpdate = new FakeIMetaUpdateService();
		var appSettings = new AppSettings
		{
			StorageFolder = "C:/storage/",
			OllamaModel = "gemma3:4b"
		};
		var sut = new ImageClassificationService(fakeOllama, fakeMetaUpdate, appSettings);

		var item = new FileIndexItem
		{
			FilePath = "/set/photo.jpg",
			FileName = "photo.jpg",
			Tags = "Cat, sky",
			RejectedTags = "night",
			SuggestedTags = "tree"
		};

		var result = await sut.ClassifyAndUpdateAsync(item, TestContext.CancellationToken);

		Assert.IsTrue(result.Success);
		Assert.Contains("mountain", item.SuggestedTags);
		Assert.DoesNotContain(item.SuggestedTags, "cat");
		Assert.DoesNotContain(item.SuggestedTags, "night");
		Assert.AreEqual("gemma3:4b", item.ImageClassificationModel);
		Assert.IsGreaterThan(default(DateTime), item.ImageClassificationGeneratedAt);
		StringAssert.EndsWith(fakeOllama.LastInferPath.Replace('\\', '/'), "/set/photo.jpg");

		Assert.HasCount(1, fakeMetaUpdate.ChangedFileIndexItemNameContent);
		var changed = fakeMetaUpdate.ChangedFileIndexItemNameContent.Single();
		Assert.IsTrue(changed.ContainsKey("/set/photo.jpg"));
		Assert.Contains("suggestedtags", changed["/set/photo.jpg"]);
	}

	[TestMethod]
	public async Task ClassifyAndUpdateAsync_ShouldReturnFailure_WhenOllamaFails()
	{
		var fakeOllama = new FakeOllamaService
		{
			InferResult = OllamaCommandResult.Failed("boom")
		};
		var fakeMetaUpdate = new FakeIMetaUpdateService();
		var sut = new ImageClassificationService(fakeOllama, fakeMetaUpdate, new AppSettings());

		var item = new FileIndexItem
		{
			FilePath = "/set/photo.jpg",
			FileName = "photo.jpg"
		};

		var result = await sut.ClassifyAndUpdateAsync(item, TestContext.CancellationToken);

		Assert.IsFalse(result.Success);
		Assert.IsEmpty(fakeMetaUpdate.ChangedFileIndexItemNameContent);
	}
}





