using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.sync.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.Helpers;

[TestClass]
public class SizeFileHashIsTheSameTest
{
	[TestMethod]
	public async Task SizeFileHashIsTheSame_true()
	{
		var lastEdited = new DateTime(2020, 03, 07, 18,
			25, 02, DateTimeKind.Local);

		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" }, new List<byte[]> { CreateAnImage.Bytes.ToArray() },
			new List<DateTime> { lastEdited });
		var (fileHash, _) =
			await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync("/test.jpg");
		var dbItems = new List<FileIndexItem>
		{
			new("/test.jpg") { FileHash = fileHash, LastEdited = lastEdited }
		};

		var sync = new SizeFileHashIsTheSameHelper(storage, new FakeIWebLogger());

		var theSame = await sync.SizeFileHashIsTheSame(dbItems, "/test.jpg");

		Assert.IsTrue(theSame.Item1);
	}

	[TestMethod]
	public async Task SizeFileHashIsTheSame_NotFoundFalse()
	{
		var sync = new SizeFileHashIsTheSameHelper(new FakeIStorage(), new FakeIWebLogger());
		var theSame = await sync.SizeFileHashIsTheSame(
			new List<FileIndexItem> { new("/not-found.jpg") { LastEdited = DateTime.Now } },
			"/not-found.jpg");

		Assert.IsFalse(theSame.Item1);
	}

	[TestMethod]
	public async Task IsTheSame_DateTime()
	{
		var lastEdited = new DateTime(2020, 03, 07,
			18, 25, 02, DateTimeKind.Local);
		var dbItems = new List<FileIndexItem> { new("/test.jpg") { DateTime = lastEdited } };
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" }, new List<byte[]> { CreateAnImage.Bytes.ToArray() },
			new List<DateTime> { lastEdited });

		var result = await new SizeFileHashIsTheSameHelper(storage, new FakeIWebLogger())
			.SizeFileHashIsTheSame(
				dbItems,
				"/101NZ_50/DSC_0045.NEF");

		Assert.IsFalse(result.Item1);
		Assert.IsFalse(result.Item2);
	}

	[TestMethod]
	public async Task IsTheSame_Hash()
	{
		var lastEdited = new DateTime(2020, 03, 07, 18,
			25, 02, DateTimeKind.Local);
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" }, new List<byte[]> { CreateAnImage.Bytes.ToArray() },
			new List<DateTime> { lastEdited });
		var (fileHash, _) =
			await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync("/test.jpg");
		var dbItems = new List<FileIndexItem> { new("/test.jpg") { FileHash = fileHash } };

		var result = await new SizeFileHashIsTheSameHelper(storage, new FakeIWebLogger())
			.SizeFileHashIsTheSame(
				dbItems,
				"/test.jpg");
		Assert.IsFalse(result.Item1);
		Assert.IsTrue(result.Item2);
	}

	[TestMethod]
	public async Task ShouldScanForXmpFile()
	{
		const string text =
			"[{\"FilePath\":\"/101NZ_50/DSC_0045.xmp\",\"FileName\":\"DSC_0045.xmp\",\"FileHash\":\"KI5OOLPPWXFL6PWPNK3KMUGXIE\"," +
			"\"FileCollectionName\":\"DSC_0045\",\"ParentDirectory\":\"/101NZ_50\",\"IsDirectory\":false,\"Tags\":\"deventer6\"," +
			"\"Status\":\"OkAndSame\",\"Description\":\"\",\"Title\":\"\",\"DateTime\":\"2020-03-07T18:25:02\"," +
			"\"AddToDatabase\":\"2023-03-10T17:21:20.070891\",\"LastEdited\":\"2023-03-14T21:56:42.3605693Z\",\"Latitude\":0,\"Longitude\":0," +
			"\"LocationAltitude\":0,\"LocationCity\":\"\",\"LocationState\":\"\",\"LocationCountry\":\"\",\"LocationCountryCode\":null," +
			"\"ColorClass\":0,\"Orientation\":\"Horizontal\",\"ImageWidth\":120,\"ImageHeight\":160,\"ImageFormat\":\"xmp\"," +
			"\"CollectionPaths\":[],\"SidecarExtensionsList\":[\"xmp\"],\"Aperture\":8,\"ShutterSpeed\":\"1/10\",\"IsoSpeed\":400," +
			"\"Software\":\"Ver.01.10\",\"MakeModel\":\"Nikon Corporation|NIKON Z 50|NIKKOR Z DX 16-50mm f/3.5-6.3 VR\",\"Make\":" +
			"\"Nikon Corporation\",\"Model\":\"NIKON Z 50\",\"LensModel\":\"NIKKOR Z DX 16-50mm f/3.5-6.3 VR\",\"FocalLength\":20," +
			"\"Size\":2437,\"ImageStabilisation\":\"Unknown\",\"LastChanged\":[\"SidecarExtensions\",\"SidecarExtensions\",\"SidecarExtensions\"," +
			"\"SidecarExtensions\",\"SidecarExtensions\"]},{\"FilePath\":\"/101NZ_50/DSC_0045.NEF\",\"FileName\":\"DSC_0045.NEF\"," +
			"\"FileHash\":\"INKV4BSQ54PIAIS5XUFAKBUW5Y\",\"FileCollectionName\":\"DSC_0045\",\"ParentDirectory\":\"/101NZ_50\"," +
			"\"IsDirectory\":false,\"Tags\":\"deventer\",\"Status\":\"OkAndSame\",\"Description\":\"\",\"Title\":\"\"," +
			"\"DateTime\":\"2020-03-07T18:25:02\",\"AddToDatabase\":\"2023-02-28T17:58:08.773932\"," +
			"\"LastEdited\":\"2020-03-07T17:25:02Z\",\"Latitude\":0,\"Longitude\":0,\"LocationAltitude\":0," +
			"\"LocationCity\":\"\",\"LocationState\":\"\",\"LocationCountry\":\"\",\"LocationCountryCode\":null," +
			"\"ColorClass\":0,\"Orientation\":\"Horizontal\",\"ImageWidth\":160,\"ImageHeight\":120," +
			"\"ImageFormat\":\"tiff\",\"CollectionPaths\":[],\"SidecarExtensionsList\":[\"xmp\"]," +
			"\"Aperture\":8,\"ShutterSpeed\":\"0,1\",\"IsoSpeed\":400,\"Software\":\"Ver.01.10\"," +
			"\"MakeModel\":\"Nikon Corporation|NIKON Z 50|NIKKOR Z DX 16-50mm f/3.5-6.3 VR\"," +
			"\"Make\":\"Nikon Corporation\",\"Model\":\"NIKON Z 50\"," +
			"\"LensModel\":\"NIKKOR Z DX 16-50mm f/3.5-6.3 VR\",\"FocalLength\":20,\"Size\":26494796,\"ImageStabilisation\":\"Unknown\",\"LastChanged\":[]}]";

		var lastEdited = new DateTime(2020, 03, 07,
			18, 25, 02, DateTimeKind.Local);
		var storage = new FakeIStorage(new List<string> { "/", "/101NZ_50" },
			new List<string> { "/101NZ_50/DSC_0045.NEF", "/101NZ_50/DSC_0045.xmp" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnGpx.Bytes.ToArray() },
			new List<DateTime> { lastEdited, lastEdited });

		var dbItems = JsonSerializer.Deserialize<List<FileIndexItem>>(text);
		Assert.IsNotNull(dbItems);
		dbItems[0].LastEdited = lastEdited;
		var (fileHash, _) =
			await new FileHash(storage, new FakeIWebLogger()).GetHashCodeAsync(
				"/101NZ_50/DSC_0045.NEF");
		dbItems[1].FileHash = fileHash;
		dbItems[1].LastEdited = lastEdited;

		var result = await new SizeFileHashIsTheSameHelper(storage, new FakeIWebLogger())
			.SizeFileHashIsTheSame(
				dbItems,
				"/101NZ_50/DSC_0045.NEF");
		Assert.IsNull(result.Item1);
		Assert.IsNull(result.Item2);
	}
}
