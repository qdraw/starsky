using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

/// <summary>
///     The jpeg profiles of the _default publish profile of appsettings.json
///     with the real overlay implementation
/// </summary>
[TestClass]
public sealed class OverlayImagePublishTest
{
	public TestContext TestContext { get; set; } = null!;

	private static string FindAppSettingsJson()
	{
		var dir = new DirectoryInfo(AppContext.BaseDirectory);
		while ( dir != null )
		{
			var candidate = Path.Combine(dir.FullName, "starsky", "appsettings.json");
			if ( File.Exists(candidate) )
			{
				return candidate;
			}

			dir = dir.Parent;
		}

		throw new FileNotFoundException("starsky/appsettings.json not found");
	}

	/// <summary>
	///     Read the jpeg profiles of _default, so the test follows changes in the configuration
	/// </summary>
	private static List<AppSettingsPublishProfiles> DefaultJpegProfiles()
	{
		using var document = JsonDocument.Parse(File.ReadAllText(FindAppSettingsJson()),
			new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
		var profiles = document.RootElement.EnumerateObject()
			.First(p => p.Name.Equals("app", StringComparison.OrdinalIgnoreCase)).Value
			.EnumerateObject()
			.First(p => p.Name.Equals("publishProfiles", StringComparison.OrdinalIgnoreCase))
			.Value.EnumerateObject()
			.First(p => p.Name == "_default").Value;

		var result = new List<AppSettingsPublishProfiles>();
		foreach ( var item in profiles.EnumerateArray() )
		{
			if ( !item.GetProperty("ContentType").GetString()!
				    .Equals("jpeg", StringComparison.OrdinalIgnoreCase) )
			{
				continue;
			}

			result.Add(new AppSettingsPublishProfiles
			{
				ContentType = TemplateContentType.Jpeg,
				SourceMaxWidth = item.GetProperty("SourceMaxWidth").GetInt32(),
				OverlayMaxWidth = item.GetProperty("OverlayMaxWidth").GetInt32(),
				Path = item.GetProperty("Path").GetString()!,
				Folder = item.GetProperty("Folder").GetString()!,
				Append = item.GetProperty("Append").GetString()!,
				MetaData = !item.TryGetProperty("MetaData", out var metaData) ||
				           metaData.ToString().Equals("true", StringComparison.OrdinalIgnoreCase)
			});
		}

		return result;
	}

	private static byte[] LogoBytes(AppSettingsPublishProfiles profile)
	{
		// profile.Path is resolved to {BaseDirectory}/WebHtmlPublish/EmbeddedViews/...
		return File.ReadAllBytes(profile.Path);
	}

	public static IEnumerable<string> Images()
	{
		yield return "landscape";
		yield return "portrait";
		yield return "small";
	}

	private static byte[] Source(string name)
	{
		return name switch
		{
			"landscape" => CreateAnSyntheticImage.Jpeg(3000, 2000),
			"portrait" => CreateAnSyntheticImage.Jpeg(1200, 1800),
			_ => new CreateAnImageWithThumbnail().Bytes.ToArray()
		};
	}

	[TestMethod]
	public void DefaultProfile_IsAsExpected()
	{
		var profiles = DefaultJpegProfiles();

		Assert.HasCount(2, profiles);
		Assert.AreEqual(1000, profiles[0].SourceMaxWidth);
		Assert.AreEqual(380, profiles[0].OverlayMaxWidth);
		Assert.AreEqual("1000/", profiles[0].Folder);
		Assert.AreEqual("_kl1k", profiles[0].Append);
		Assert.EndsWith("qdrawlarge.png", profiles[0].Path);
		Assert.AreEqual(500, profiles[1].SourceMaxWidth);
		Assert.AreEqual(200, profiles[1].OverlayMaxWidth);
		Assert.AreEqual("500/", profiles[1].Folder);
		Assert.AreEqual("_kl", profiles[1].Append);
		Assert.EndsWith("qdrawsmall.png", profiles[1].Path);
		Assert.IsTrue(File.Exists(profiles[0].Path), profiles[0].Path);
		Assert.IsTrue(File.Exists(profiles[1].Path), profiles[1].Path);
	}

	private static async Task<byte[]> Overlay(byte[] source, AppSettingsPublishProfiles profile)
	{
		var storage = new FakeIStorage(["/"], ["/test.jpg", profile.Path],
			new List<byte[]> { source, LogoBytes(profile) });
		var overlay = new OverlayImageSkia(new FakeSelectorStorage(storage), new AppSettings());

		var written = await overlay.ResizeOverlayImageLarge("/test.jpg", "/out.jpg", profile);
		Assert.IsTrue(written);

		using var output = storage.ReadStream("/out.jpg");
		using var memory = new MemoryStream();
		await output.CopyToAsync(memory);
		return memory.ToArray();
	}

	[TestMethod]
	[DynamicData(nameof(Images))]
	public async Task ResizeOverlayImageLarge_DefaultProfile_IsResizedJpeg(string name)
	{
		var source = Source(name);
		using var sourceImage = ImageCompare.Decode(source);

		foreach ( var profile in DefaultJpegProfiles() )
		{
			var output = await Overlay(source, profile);
			Assert.IsTrue(ImageCompare.IsJpeg(output));

			using var image = ImageCompare.Decode(output);
			Assert.AreEqual(profile.SourceMaxWidth, image.Width, name);
			var expectedHeight =
				( int ) Math.Round(sourceImage.Height * ( double ) profile.SourceMaxWidth /
				                   sourceImage.Width);
			Assert.AreEqual(expectedHeight, image.Height, 1, name);
		}
	}

	[TestMethod]
	[DynamicData(nameof(Images))]
	public async Task ResizeOverlayImageLarge_Skia_LogoIsDrawnBottomRight(string name)
	{
		var source = Source(name);
		foreach ( var profile in DefaultJpegProfiles() )
		{
			var skia = await Overlay(source, profile);
			using var output = ImageCompare.Decode(skia);

			// same pipeline without the logo
			using var sourceStream = new MemoryStream(source);
			using var oriented = SkiaImageHelper.DecodeAutoOrient(sourceStream);
			using var plain = SkiaImageHelper.Resize(oriented, profile.SourceMaxWidth, 0);

			using var logoStream = new MemoryStream(LogoBytes(profile));
			using var logoOriginal = SkiaImageHelper.Decode(logoStream);
			using var logo = SkiaImageHelper.Resize(logoOriginal, profile.OverlayMaxWidth, 0);

			var logoArea = new SKRectI(output.Width - logo.Width, output.Height - logo.Height,
				output.Width, output.Height);
			var topLeft = new SKRectI(0, 0, 50, 50);

			var logoDifference = ImageCompare.MeanAbsoluteDifference(output, plain, logoArea);
			var untouchedDifference =
				ImageCompare.MeanAbsoluteDifference(output, plain, topLeft);

			TestContext.WriteLine(
				$"{name} {profile.Folder}: logo area {logoDifference:F2} untouched {untouchedDifference:F2}");
			Assert.IsGreaterThan(untouchedDifference, logoDifference);
			Assert.IsGreaterThan(3, logoDifference);
		}
	}

	/// <summary>
	///     The publish joins the output root and the profile folder with the platform separator
	/// </summary>
	/// <param name="relativePath">for example 1000/test_kl1k.jpg</param>
	private static string OutputRootPath(string relativePath)
	{
		return PathHelper.AddBackslash("/") + relativePath;
	}

	/// <summary>
	///     Run the real publish (GenerateJpeg) for the _default profile
	/// </summary>
	private static async Task<Dictionary<string, byte[]>> Publish(byte[] source)
	{
		var profiles = DefaultJpegProfiles();
		var appSettings = new AppSettings
		{
			PublishProfiles = new Dictionary<string, List<AppSettingsPublishProfiles>>
			{
				{ "_default", profiles }
			}
		};

		var files = new List<string> { "/test.jpg" };
		var bytes = new List<byte[]> { source };
		foreach ( var profile in profiles )
		{
			files.Add(profile.Path);
			bytes.Add(LogoBytes(profile));
		}

		var storage = new FakeIStorage(["/"], files, bytes);
		var selectorStorage = new FakeSelectorStorage(storage);
		var consoleWrapper = new FakeConsoleWrapper();
		var service = new WebHtmlPublishService(new PublishPreflight(appSettings,
				consoleWrapper, selectorStorage, new FakeIWebLogger()),
			selectorStorage, appSettings, new FakeExifTool(storage, appSettings),
			new OverlayImageSkia(selectorStorage, appSettings), consoleWrapper, new FakeIWebLogger(),
			new FakeIThumbnailService(selectorStorage), new FakeImageOptimisationService());

		var result = new Dictionary<string, byte[]>();
		var publishProfiles = new PublishPreflight(appSettings, consoleWrapper,
			selectorStorage, new FakeIWebLogger()).GetPublishProfileName("_default");
		foreach ( var profile in publishProfiles.Where(p =>
			         p.ContentType == TemplateContentType.Jpeg) )
		{
			var generated = await service.GenerateJpeg(profile,
				new List<FileIndexItem> { new("/test.jpg") }, "/", 1);
			Assert.IsTrue(generated.Values.All(v => v));

			var relativePath = profile.Folder + "test" + profile.Append + ".jpg";
			var outputPath = OutputRootPath(relativePath);
			Assert.IsTrue(storage.ExistFile(outputPath), outputPath);
			using var stream = storage.ReadStream(outputPath);
			using var memory = new MemoryStream();
			await stream.CopyToAsync(memory);
			result[relativePath] = memory.ToArray();
		}

		return result;
	}

	[TestMethod]
	[DynamicData(nameof(Images))]
	public async Task GenerateJpeg_DefaultPublishProfile(string name)
	{
		var source = Source(name);
		var published = await Publish(source);

		Assert.HasCount(2, published);
		Assert.IsTrue(published.ContainsKey("1000/test_kl1k.jpg"));
		Assert.IsTrue(published.ContainsKey("500/test_kl.jpg"));

		foreach ( var (key, bytes) in published )
		{
			Assert.IsTrue(ImageCompare.IsJpeg(bytes), key);
			using var image = ImageCompare.Decode(bytes);
			Assert.AreEqual(key.StartsWith("1000/", StringComparison.Ordinal) ? 1000 : 500,
				image.Width, key);
		}
	}
}
