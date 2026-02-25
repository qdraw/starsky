using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.foundation.optimisation.Models;

[TestClass]
public class ImageOptimisationBinariesIndexTests
{
	[TestMethod]
	public void Constructor_WithValidJson_ParsesData()
	{
		var json = JsonSerializer.Serialize(new ImageOptimisationBinariesIndex
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

		var container = new ImageOptimisationBinariesContainer(
			json,
			new Uri("https://starsky-dependencies.netlify.app/mozjpeg/index.json"),
			[new Uri("https://starsky-dependencies.netlify.app/mozjpeg/")],
			true);

		Assert.IsTrue(container.Success);
		Assert.IsNotNull(container.Data);
		Assert.HasCount(1, container.Data.Binaries);
		Assert.AreEqual("linux-x64", container.Data.Binaries[0].Architecture);
	}

	[TestMethod]
	public void Constructor_WithEmptyJson_DataIsNull()
	{
		var container = new ImageOptimisationBinariesContainer(
			string.Empty,
			null,
			new List<Uri>(),
			false);

		Assert.IsFalse(container.Success);
		Assert.IsNull(container.Data);
	}

	[TestMethod]
	public void Constructor_Default_BaseUrlsInitialized()
	{
		var container = new ImageOptimisationBinariesContainer();

		Assert.IsNotNull(container.BaseUrls);
		Assert.IsEmpty(container.BaseUrls);
	}
}
