using System.Collections.Generic;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Jpeg;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailmeta.ServicesTinySize;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageA330Raw;
using starskytest.FakeCreateAn.CreateAnImageA6600Raw;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailmeta.ServicesTinySize;

[TestClass]
public sealed class OffsetDataMetaExifThumbnailTest
{
	[TestMethod]
	public void ParseMetaThumbnail_Null()
	{
		var (exifThumbnailDirectory, width, height, rotation) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(),
			new FakeIWebLogger()).ParseMetaThumbnail(null, null);
		Assert.AreEqual(null, exifThumbnailDirectory);
		Assert.AreEqual(0, width);
		Assert.AreEqual(0, height);
		Assert.AreEqual(RotationModel.Rotation.DoNotChange, rotation);
	}

	[TestMethod]
	public void ParseMetaThumbnail_ParseMetaThumbnail_NoValidSize_DueWrongFormat_Png()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.png", "/test.jpg" },
			new List<byte[]>
			{
				CreateAnPng.Bytes.ToArray(), new CreateAnImageWithThumbnail().Bytes.ToArray()
			});

		var (allExifItems, _) = new OffsetDataMetaExifThumbnail(new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ReadExifMetaDirectories("/test.png");
		var (_, thumbnailDirectory) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ReadExifMetaDirectories("/test.jpg");

		// Switch around to get a situation where the image has no size but an valid thumbnail
		var (exifThumbnailDirectory, width, height, rotation) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ParseMetaThumbnail(allExifItems, thumbnailDirectory);

		Assert.AreEqual(thumbnailDirectory, exifThumbnailDirectory);
		Assert.AreEqual(0, width);
		Assert.AreEqual(0, height);
		Assert.AreEqual(RotationModel.Rotation.DoNotChange, rotation);
	}

	/// <summary>
	///     Happy flow
	/// </summary>
	[TestMethod]
	public void ParseMetaThumbnail_CheckJpeg_Success()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.png", "/test.jpg" },
			new List<byte[]>
			{
				CreateAnPng.Bytes.ToArray(), new CreateAnImageWithThumbnail().Bytes.ToArray()
			});

		var (allExifItems, thumbnailDirectory) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ReadExifMetaDirectories("/test.jpg");

		var (exifThumbnailDirectory, width, height, rotation) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ParseMetaThumbnail(allExifItems, thumbnailDirectory);

		Assert.AreEqual(thumbnailDirectory, exifThumbnailDirectory);
		Assert.AreEqual(150, width);
		Assert.AreEqual(100, height);
		Assert.AreEqual(RotationModel.Rotation.Horizontal, rotation);
	}

	/// <summary>
	///     Happy flow RAW
	/// </summary>
	[TestMethod]
	[DataRow("/A3330.arw", 3880, 2600)]
	[DataRow("/A6600.arw", 6000, 4000)]
	public void ParseMetaThumbnail_CheckRaw_Success(string fileName, int expectedWidth,
		int expectedHeight)
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/A6600.arw", "/A3330.arw" },
			new List<byte[]>
			{
				new CreateAnImageA6600Raw().Bytes.ToArray(),
				new CreateAnImageA330Raw().Bytes.ToArray()
			});

		var (allExifItems, thumbnailDirectory) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ReadExifMetaDirectories(fileName);

		var service = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger());
		var (exifThumbnailDirectory, width, height, rotation) =
			service.ParseMetaThumbnail(allExifItems, thumbnailDirectory);

		Assert.AreEqual(thumbnailDirectory, exifThumbnailDirectory);
		Assert.AreEqual(expectedWidth, width);
		Assert.AreEqual(expectedHeight, height);
		Assert.AreEqual(RotationModel.Rotation.Horizontal, rotation);
	}

	[TestMethod]
	public void OffsetDataMetaExifThumbnail_ExifSubIfdDirectory()
	{
		var container = new List<Directory>();
		var dir2 = new ExifSubIfdDirectory();
		dir2.Set(ExifDirectoryBase.TagExifImageHeight, "10");
		dir2.Set(ExifDirectoryBase.TagExifImageWidth, "12");
		container.Add(dir2);
		var dir3 = new ExifIfd0Directory();
		dir3.Set(ExifDirectoryBase.TagOrientation, 6);
		container.Add(dir3);
		var storage = new FakeIStorage();

		var service = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger());
		var (_, width, height, rotation) =
			service.ParseMetaThumbnail(container, new ExifThumbnailDirectory(1));

		Assert.AreEqual(12, width);
		Assert.AreEqual(10, height);
		Assert.AreEqual(RotationModel.Rotation.Rotate90Cw, rotation);
	}

	[TestMethod]
	public void OffsetDataMetaExifThumbnail_JpegDirectory()
	{
		var container = new List<Directory>();
		var dir2 = new JpegDirectory();
		dir2.Set(JpegDirectory.TagImageHeight, "10");
		dir2.Set(JpegDirectory.TagImageWidth, "12");
		container.Add(dir2);
		var dir3 = new ExifIfd0Directory();
		dir3.Set(ExifDirectoryBase.TagOrientation, 6);
		container.Add(dir3);
		var storage = new FakeIStorage();

		var (_, width, height, rotation) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ParseMetaThumbnail(container, new ExifThumbnailDirectory(1));

		Assert.AreEqual(12, width);
		Assert.AreEqual(10, height);
		Assert.AreEqual(RotationModel.Rotation.Rotate90Cw, rotation);
	}

	[TestMethod]
	public void OffsetDataMetaExifThumbnail_MissingJpegDirectory()
	{
		var container = new List<Directory>();
		var dir2 = new JpegDirectory();
		container.Add(dir2);
		var dir3 = new ExifIfd0Directory();
		container.Add(dir3);
		var storage = new FakeIStorage();

		var (_, width, height, rotation) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ParseMetaThumbnail(container, new ExifThumbnailDirectory(1));

		Assert.AreEqual(0, width);
		Assert.AreEqual(0, height);
		Assert.AreEqual(RotationModel.Rotation.DoNotChange, rotation);
	}

	[TestMethod]
	public void ParseOffsetData_TagThumbnailLengthWrongData()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { new CreateAnImageWithThumbnail().Bytes.ToArray() });

		var (_, thumbnailDirectory) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ReadExifMetaDirectories("/test.jpg");

		// overwrite to set a wrong value
		thumbnailDirectory?.Set(ExifThumbnailDirectory.TagThumbnailLength, 1);

		var offsetData = new OffsetDataMetaExifThumbnail(new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ParseOffsetData(thumbnailDirectory, "/test.jpg");

		Assert.IsFalse(offsetData.Success);
		Assert.IsNotNull(offsetData.Reason);
	}

	[TestMethod]
	public void ParseOffsetData_Success()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { new CreateAnImageWithThumbnail().Bytes.ToArray() });

		var (_, thumbnailDirectory) = new OffsetDataMetaExifThumbnail(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ReadExifMetaDirectories("/test.jpg");

		var offsetData = new OffsetDataMetaExifThumbnail(new FakeSelectorStorage(storage),
			new FakeIWebLogger()).ParseOffsetData(thumbnailDirectory, "/test.jpg");

		Assert.IsTrue(offsetData.Success);
	}
}
