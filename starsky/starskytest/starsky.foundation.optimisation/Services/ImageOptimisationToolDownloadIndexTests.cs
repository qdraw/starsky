using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Models;
using starsky.foundation.optimisation.Services;
using starsky.foundation.platform.JsonConverter;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.optimisation.Services;

[TestClass]
public class ImageOptimisationToolDownloadIndexTests
{
	[TestMethod]
	public async Task DownloadIndex_UsesFirstIndexUrl_WhenAvailable()
	{
		var payload = JsonSerializer.Serialize(new ImageOptimisationBinariesIndex
		{
			Binaries =
			[
				new ImageOptimisationBinaryIndex
				{
					Architecture = "linux-x64",
					FileName = "mozjpeg-linux-x64.zip",
					Sha256 = "abc"
				}
			]
		}, DefaultJsonSerializer.CamelCase);

		var first = new Uri("https://starsky-dependencies.netlify.app/mozjpeg/index.json");
		var second = new Uri("https://qdraw.nl/special/mirror/mozjpeg/index.json");
		var http = new FakeIHttpClientHelper(new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{ first.ToString(), new KeyValuePair<bool, string>(true, payload) }
			});
		var sut = new ImageOptimisationToolDownloadIndex(http, new FakeIWebLogger());

		var result = await sut.DownloadIndex(new ImageOptimisationToolDownloadOptions
		{
			ToolName = "mozjpeg",
			IndexUrls = [first, second],
			BaseUrls =
			[
				new Uri("https://starsky-dependencies.netlify.app/mozjpeg/"),
				new Uri("https://qdraw.nl/special/mirror/mozjpeg/")
			]
		});

		Assert.IsTrue(result.Success);
		Assert.AreEqual(first, result.IndexUrl);
		Assert.IsNotNull(result.Data);
	}

	[TestMethod]
	public async Task DownloadIndex_UsesSecondIndexUrl_WhenFirstFails()
	{
		var payload = JsonSerializer.Serialize(new ImageOptimisationBinariesIndex
		{
			Binaries =
			[
				new ImageOptimisationBinaryIndex
				{
					Architecture = "linux-x64",
					FileName = "mozjpeg-linux-x64.zip",
					Sha256 = "abc"
				}
			]
		}, DefaultJsonSerializer.CamelCase);

		var first = new Uri("https://starsky-dependencies.netlify.app/mozjpeg/index.json");
		var second = new Uri("https://qdraw.nl/special/mirror/mozjpeg/index.json");
		var http = new FakeIHttpClientHelper(new FakeIStorage(),
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{ second.ToString(), new KeyValuePair<bool, string>(true, payload) }
			});
		var sut = new ImageOptimisationToolDownloadIndex(http, new FakeIWebLogger());

		var result = await sut.DownloadIndex(new ImageOptimisationToolDownloadOptions
		{
			ToolName = "mozjpeg",
			IndexUrls = [first, second],
			BaseUrls =
			[
				new Uri("https://starsky-dependencies.netlify.app/mozjpeg/"),
				new Uri("https://qdraw.nl/special/mirror/mozjpeg/")
			]
		});

		Assert.IsTrue(result.Success);
		Assert.AreEqual(second, result.IndexUrl);
		Assert.IsNotNull(result.Data);
	}

	[TestMethod]
	public async Task DownloadIndex_ReturnsFailed_WhenBothFail()
	{
		var first = new Uri("https://starsky-dependencies.netlify.app/mozjpeg/index.json");
		var second = new Uri("https://qdraw.nl/special/mirror/mozjpeg/index.json");
		var sut = new ImageOptimisationToolDownloadIndex(
			new FakeIHttpClientHelper(new FakeIStorage(),
				new Dictionary<string, KeyValuePair<bool, string>>()),
			new FakeIWebLogger());

		var result = await sut.DownloadIndex(new ImageOptimisationToolDownloadOptions
		{
			ToolName = "mozjpeg",
			IndexUrls = [first, second],
			BaseUrls =
			[
				new Uri("https://starsky-dependencies.netlify.app/mozjpeg/"),
				new Uri("https://qdraw.nl/special/mirror/mozjpeg/")
			]
		});

		Assert.IsFalse(result.Success);
		Assert.IsNull(result.Data);
	}
}
