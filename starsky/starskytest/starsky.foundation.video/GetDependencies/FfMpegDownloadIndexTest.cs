using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegDownloadIndexTest
{
	private readonly FfmpegBinariesIndex _example;
	private readonly FakeIHttpClientHelper _httpClientHelperFirstSource;
	private readonly FakeIHttpClientHelper _httpClientHelperSecondSource;

	private readonly FakeIStorage _storage = new();

	public FfMpegDownloadIndexTest()
	{
		_example = new FfmpegBinariesIndex
		{
			Binaries = new List<BinaryIndex>
			{
				new()
				{
					Architecture = "win-x64",
					FileName = "test.zip",
					Sha256 = "test-sha256"
				},
				new()
				{
					Architecture = "osx-x64",
					FileName = "test.zip",
					Sha256 = "test-sha256"
				},
				new()
				{
					Architecture = "linux-x64",
					FileName = "test.zip",
					Sha256 = "test-sha256"
				}
			}
		};

		_httpClientHelperFirstSource =
			new FakeIHttpClientHelper(_storage,
				new Dictionary<string, KeyValuePair<bool, string>>
				{
					{
						FfMpegDownloadIndex.FfMpegApiIndex.ToString(),
						new KeyValuePair<bool, string>(
							true,
							JsonSerializer.Serialize(_example, DefaultJsonSerializer.CamelCase))
					}
				});

		_httpClientHelperSecondSource =
			new FakeIHttpClientHelper(_storage,
				new Dictionary<string, KeyValuePair<bool, string>>
				{
					{
						FfMpegDownloadIndex.FfMpegApiIndexMirror.ToString(),
						new KeyValuePair<bool, string>(true,
							JsonSerializer.Serialize(_example, DefaultJsonSerializer.CamelCase))
					}
				});
	}

	[TestMethod]
	[DataRow(1)]
	[DataRow(2)]
	public async Task FfMpegDownloadIndexTest_WithExampleData_Success(int index)
	{
		var ffMpegDownloadIndex = index switch
		{
			1 => new FfMpegDownloadIndex(_httpClientHelperFirstSource, new FakeIWebLogger()),
			2 => new FfMpegDownloadIndex(_httpClientHelperSecondSource, new FakeIWebLogger()),
			_ => null
		};

		var result = await ffMpegDownloadIndex!.DownloadIndex();

		Assert.IsNotNull(result);

		Assert.AreEqual(_example.Binaries.Count, result.Data?.Binaries.Count);
	}

	[TestMethod]
	public async Task FfMpegDownloadIndexTest_NotFound()
	{
		// Arrange
		var sut = new FfMpegDownloadIndex(
			new FakeIHttpClientHelper(new FakeIStorage(),
				new Dictionary<string, KeyValuePair<bool, string>>()), new FakeIWebLogger());
		
		// Act
		var result = await sut.DownloadIndex();
		
		// Assert
		Assert.IsNull(result.Data);
		Assert.IsFalse(result.Success);
	}
}
