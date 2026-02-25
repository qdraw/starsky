using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.optimisation.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.ArchiveFormats.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.optimisation.Services;

[TestClass]
public class ImageOptimisationToolDownloadTests
{
	private static readonly ImageOptimisationToolDownloadOptions OptionsNoChmod = new()
	{
		ToolName = "mozjpeg",
		RunChmodOnUnix = false,
		IndexUrls =
		[
			new Uri("https://starsky-dependencies.netlify.app/mozjpeg/index.json"),
			new Uri("https://qdraw.nl/special/mirror/mozjpeg/index.json")
		],
		BaseUrls =
		[
			new Uri("https://starsky-dependencies.netlify.app/mozjpeg/"),
			new Uri("https://qdraw.nl/special/mirror/mozjpeg/")
		]
	};

	private static readonly ImageOptimisationToolDownloadOptions OptionsWithChmod = new()
	{
		ToolName = "mozjpeg",
		RunChmodOnUnix = true,
		IndexUrls =
		[
			new Uri("https://starsky-dependencies.netlify.app/mozjpeg/index.json"),
			new Uri("https://qdraw.nl/special/mirror/mozjpeg/index.json")
		],
		BaseUrls =
		[
			new Uri("https://starsky-dependencies.netlify.app/mozjpeg/"),
			new Uri("https://qdraw.nl/special/mirror/mozjpeg/")
		]
	};

	private readonly AppSettings _appSettings = new();
	private readonly FakeIWebLogger _logger = new();

	[TestMethod]
	public async Task Download_ReturnsDownloadIndexFailed_WhenIndexFails()
	{
		var storage = new FakeIStorage();
		var sut = CreateSut(storage,
			new FakeIHttpClientHelper(storage,
				new Dictionary<string, KeyValuePair<bool, string>>()),
			new FakeImageOptimisationToolDownloadIndex
			{
				Result = new ImageOptimisationBinariesContainer(string.Empty, null, [], false)
			},
			new Zipper(new FakeIWebLogger()));

		var result = await sut.Download(OptionsNoChmod, "linux-x64");

		Assert.AreEqual(ImageOptimisationDownloadStatus.DownloadIndexFailed, result);
	}

	[TestMethod]
	public async Task Download_ReturnsMissingFileName_WhenArchitectureNotInIndex()
	{
		var storage = new FakeIStorage();
		var sut = CreateSut(storage,
			new FakeIHttpClientHelper(storage,
				new Dictionary<string, KeyValuePair<bool, string>>()),
			new FakeImageOptimisationToolDownloadIndex
			{
				Result = new ImageOptimisationBinariesContainer
				{
					Success = true,
					Data = new ImageOptimisationBinariesIndex
					{
						Binaries =
						[
							new ImageOptimisationBinaryIndex
							{
								Architecture = "win-x64",
								FileName = "mozjpeg-win-x64.zip",
								Sha256 = "abc"
							}
						]
					},
					BaseUrls = [new Uri("https://starsky-dependencies.netlify.app/mozjpeg/")]
				}
			},
			new Zipper(new FakeIWebLogger()));

		var result = await sut.Download(OptionsNoChmod, "linux-x64");

		Assert.AreEqual(ImageOptimisationDownloadStatus.DownloadBinariesFailedMissingFileName,
			result);
	}

	[TestMethod]
	public async Task Download_ReturnsDownloadFailed_WhenAllMirrorsFail()
	{
		var storage = new FakeIStorage();
		var sut = CreateSut(storage,
			new FakeIHttpClientHelper(storage,
				new Dictionary<string, KeyValuePair<bool, string>>()),
			CreateIndex("abc"),
			new Zipper(new FakeIWebLogger()));

		var result = await sut.Download(OptionsNoChmod, "linux-x64");

		Assert.AreEqual(ImageOptimisationDownloadStatus.DownloadBinariesFailed, result);
	}

	[TestMethod]
	public async Task Download_ReturnsShaFailed_WhenChecksumDoesNotMatch()
	{
		var storage = new FakeIStorage();
		var http = new FakeIHttpClientHelper(storage,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://starsky-dependencies.netlify.app/mozjpeg/mozjpeg-linux-x64.zip",
					new KeyValuePair<bool, string>(true, "VGVzdENvbnRlbnQ=")
				}
			});
		var sut = CreateSut(storage, http, CreateIndex("wrong-sha"),
			new Zipper(new FakeIWebLogger()));

		var result = await sut.Download(OptionsNoChmod, "linux-x64");

		Assert.AreEqual(ImageOptimisationDownloadStatus.DownloadBinariesFailedSha256Check, result);
	}

	[TestMethod]
	public async Task Download_ReturnsExtractFailed_WhenZipCannotBeExtracted()
	{
		var storage = new FakeIStorage();
		var http = new FakeIHttpClientHelper(storage,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://starsky-dependencies.netlify.app/mozjpeg/mozjpeg-linux-x64.zip",
					new KeyValuePair<bool, string>(true, "VGVzdENvbnRlbnQ=")
				}
			});
		var validSha = "b98fc09ac0df3bbc1ee5e79316604f7462fffdf095c1c676e3c2517773645fe9";
		var sut = CreateSut(storage, http, CreateIndex(validSha),
			new FakeIZipper([], storage));

		var result = await sut.Download(OptionsNoChmod, "linux-x64");

		Assert.AreEqual(ImageOptimisationDownloadStatus.DownloadBinariesFailedZipperNotExtracted,
			result);
	}

	[TestMethod]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
	public async Task Download_ReturnsRunChmodFailed_WhenUnixAndChmodMissing__MacLinuxOnly()
	{
		const string architecture = "linux-x64";
		var zipBytes = CreateZipBytes("mozjpeg", "tool-binary");
		var zipSha = Sha256.ComputeSha256(zipBytes);
		var storage = new FakeIStorage();
		var zipFullFilePath = Path.Combine("/dependencies", "mozjpeg", "mozjpeg-linux-x64.zip");

		var http = new FakeIHttpClientHelper(storage,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://starsky-dependencies.netlify.app/mozjpeg/mozjpeg-linux-x64.zip",
					new KeyValuePair<bool, string>(true, Convert.ToBase64String(zipBytes))
				}
			});

		var sut = CreateSut(storage, http, CreateIndex(zipSha),
			new FakeIZipper([new Tuple<string, byte[]>(zipFullFilePath, zipBytes)], storage));

		var result = await sut.Download(OptionsWithChmod, architecture);

		Assert.AreEqual(ImageOptimisationDownloadStatus.RunChmodFailed, result);
	}

	[TestMethod]
	public async Task Download_ReturnsOk_WhenDownloadShaAndExtract_FileNotFoundInZip()
	{
		var storage = new FakeIStorage();
		var http = new FakeIHttpClientHelper(storage,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://starsky-dependencies.netlify.app/mozjpeg/mozjpeg-linux-x64.zip",
					new KeyValuePair<bool, string>(true, "VGVzdENvbnRlbnQ=")
				}
			});
		const string validSha = "b98fc09ac0df3bbc1ee5e79316604f7462fffdf095c1c676e3c2517773645fe9";
		var toolFolder = Path.Combine("/dependencies", "mozjpeg");
		var zipPath = Path.Combine(toolFolder, "mozjpeg-linux-x64.zip");

		var sut = CreateSut(storage, http, CreateIndex(validSha),
			new FakeIZipper(
			[
				new Tuple<string, byte[]>(zipPath,
					[80, 75, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0])
			], storage));

		var result = await sut.Download(OptionsNoChmod, "linux-x64");

		Assert.AreEqual(ImageOptimisationDownloadStatus.DownloadBinariesFailedZipperNotExtracted,
			result);
	}

	[TestMethod]
	public async Task Download_ReturnsOk_WhenToolAlreadyExists()
	{
		const string architecture = "linux-x64";
		var expectedBinaryPath = Path.Combine("/dependencies", "mozjpeg-linux-x64", "mozjpeg");
		var storage = new FakeIStorage(null, [expectedBinaryPath]);
		var sut = CreateSut(storage,
			new FakeIHttpClientHelper(storage,
				new Dictionary<string, KeyValuePair<bool, string>>()),
			CreateIndex("abc"),
			new Zipper(new FakeIWebLogger()));

		var result = await sut.Download(OptionsNoChmod, architecture);

		Assert.AreEqual(ImageOptimisationDownloadStatus.OkAlreadyDownloaded, result);
	}

	[TestMethod]
	public async Task Download_ReturnsOk_WhenDownloadShaAndExtract_Succeeds()
	{
		const string architecture = "linux-x64";
		var zipBytes = CreateZipBytes("mozjpeg", "tool-binary");
		var zipSha = Sha256.ComputeSha256(zipBytes);
		var storage = new FakeIStorage();
		var zipFullFilePath = Path.Combine("/dependencies", "mozjpeg", "mozjpeg-linux-x64.zip");

		var http = new FakeIHttpClientHelper(storage,
			new Dictionary<string, KeyValuePair<bool, string>>
			{
				{
					"https://starsky-dependencies.netlify.app/mozjpeg/mozjpeg-linux-x64.zip",
					new KeyValuePair<bool, string>(true, Convert.ToBase64String(zipBytes))
				}
			});

		var sut = CreateSut(storage, http, CreateIndex(zipSha),
			new FakeIZipper([new Tuple<string, byte[]>(zipFullFilePath, zipBytes)], storage));

		var result = await sut.Download(OptionsNoChmod, architecture);

		Assert.AreEqual(ImageOptimisationDownloadStatus.Ok, result);
	}

	private static byte[] CreateZipBytes(string entryName, string content)
	{
		using var memory = new MemoryStream();
		using ( var archive = new ZipArchive(memory, ZipArchiveMode.Create, true) )
		{
			var entry = archive.CreateEntry(entryName);
			using var entryStream = entry.Open();
			using var writer = new StreamWriter(entryStream);
			writer.Write(content);
		}

		return memory.ToArray();
	}

	private ImageOptimisationToolDownload CreateSut(FakeIStorage storage,
		FakeIHttpClientHelper http,
		IImageOptimisationToolDownloadIndex index,
		IZipper zipper)
	{
		_appSettings.DependenciesFolder = "/dependencies";
		return new ImageOptimisationToolDownload(new FakeSelectorStorage(storage), http,
			_appSettings, _logger, index, zipper,
			new ImageOptimisationChmod(new FakeSelectorStorage(storage), _logger));
	}

	private static FakeImageOptimisationToolDownloadIndex CreateIndex(string binarySha)
	{
		return new FakeImageOptimisationToolDownloadIndex
		{
			Result = new ImageOptimisationBinariesContainer
			{
				Success = true,
				Data = new ImageOptimisationBinariesIndex
				{
					Binaries =
					[
						new ImageOptimisationBinaryIndex
						{
							Architecture = "linux-x64",
							FileName = "mozjpeg-linux-x64.zip",
							Sha256 = binarySha
						}
					]
				},
				BaseUrls =
				[
					new Uri("https://starsky-dependencies.netlify.app/mozjpeg/"),
					new Uri("https://qdraw.nl/special/mirror/mozjpeg/")
				]
			}
		};
	}


	[TestMethod]
	public async Task Download_MultipleArchitectures_ReturnsStatuses()
	{
		// Arrange
		var storage = new FakeIStorage();
		var sut = CreateSut(storage,
			new FakeIHttpClientHelper(storage,
				new Dictionary<string, KeyValuePair<bool, string>>()),
			new FakeImageOptimisationToolDownloadIndex
			{
				Result = new ImageOptimisationBinariesContainer
				{
					Success = true,
					Data = new ImageOptimisationBinariesIndex
					{
						Binaries =
						[
							new ImageOptimisationBinaryIndex
							{
								Architecture = "win-x64",
								FileName = "mozjpeg-win-x64.zip",
								Sha256 = "abc"
							},
							new ImageOptimisationBinaryIndex
							{
								Architecture = "osx-x64",
								FileName = "mozjpeg-osx-x64.zip",
								Sha256 = "abc"
							}
						]
					},
					BaseUrls = [new Uri("https://starsky-dependencies.netlify.app/mozjpeg/")]
				}
			},
			new Zipper(new FakeIWebLogger()));


		var options = new ImageOptimisationToolDownloadOptions
		{
			ToolName = "mozjpeg",
			RunChmodOnUnix = false,
			IndexUrls = new List<Uri> { new("https://example.com/") },
			BaseUrls = new List<Uri> { new("https://example.com/") }
		};
		var architectures = new List<string> { "osx-x64", "win-x64" };

		// Act
		var result = await sut.Download(options, architectures);

		// Assert
		Assert.HasCount(2, result);
		Assert.IsTrue(result.All(x =>
			x is ImageOptimisationDownloadStatus.DownloadBinariesFailed));
	}

	private sealed class
		FakeImageOptimisationToolDownloadIndex : IImageOptimisationToolDownloadIndex
	{
		public required ImageOptimisationBinariesContainer Result { get; init; }

		public Task<ImageOptimisationBinariesContainer> DownloadIndex(
			ImageOptimisationToolDownloadOptions options)
		{
			return Task.FromResult(Result);
		}
	}
}
