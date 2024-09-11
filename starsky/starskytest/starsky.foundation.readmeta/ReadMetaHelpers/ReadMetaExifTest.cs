using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using MetadataExtractor.Formats.Xmp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using XmpCore.Impl;
using Directory = MetadataExtractor.Directory;

namespace starskytest.starsky.foundation.readmeta.ReadMetaHelpers;

[TestClass]
public class ReadMetaExifTest
{
	[TestMethod]
	public void GetXmpGeoAlt_PositiveAltitude_ReturnsPositiveAltitude()
	{
		// Arrange
		var dir2 = new XmpDirectory();
		dir2.SetXmpMeta(new XmpMeta());
		dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitude", "1/1");
		dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitudeRef", "0");

		var allExifItems = new List<Directory> { dir2 };

		// Act
		var result = ReadMetaExif.GetXmpGeoAlt(allExifItems);

		// Assert
		Assert.AreEqual(1, result, 0.001); // Use an appropriate tolerance
	}

	[TestMethod]
	public void GetXmpGeoAlt_NegativeAltitude_ReturnsNegativeAltitude()
	{
		// Arrange
		var dir2 = new XmpDirectory();
		dir2.SetXmpMeta(new XmpMeta());
		dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitude", "10/1");
		dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitudeRef", "1");

		var allExifItems = new List<Directory> { dir2 };

		// Act
		var result = ReadMetaExif.GetXmpGeoAlt(allExifItems);

		// Assert
		Assert.AreEqual(-10, result, 0.001); // Use an appropriate tolerance
	}

	[TestMethod]
	public void GetXmpGeoAlt_ZeroAltitude_ReturnsZeroAltitude()
	{
		// Arrange
		var dir2 = new XmpDirectory();
		dir2.SetXmpMeta(new XmpMeta());
		dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitude", "0/1");
		dir2.XmpMeta?.SetProperty("http://ns.adobe.com/exif/1.0/", "GPSAltitudeRef", "1");
		var allExifItems = new List<Directory> { dir2 };

		// Act
		var result = ReadMetaExif.GetXmpGeoAlt(allExifItems);

		// Assert
		Assert.AreEqual(0, result, 0.001); // Use an appropriate tolerance
	}

	[TestMethod]
	public void GetIsoSpeedValue_FromExifSubIfd_ReturnsIsoSpeed()
	{
		// Arrange
		var subIfdItem = new ExifSubIfdDirectory();
		subIfdItem.Set(ExifDirectoryBase.TagIsoEquivalent, "400");

		var allExifItems = new List<Directory> { subIfdItem };

		// Act
		var result = ReadMetaExif.GetIsoSpeedValue(allExifItems);

		// Assert
		Assert.AreEqual(400, result);
	}

	[TestMethod]
	public void GetIsoSpeedValue_FromCanonMakerNote_ReturnsIsoSpeed()
	{
		// Arrange
		var canonMakerNoteDirectory = new CanonMakernoteDirectory();

		// Magic Numbers
		// 19 = ISO 400
		canonMakerNoteDirectory.Set(CanonMakernoteDirectory.CameraSettings.TagIso, "19");

		var allExifItems = new List<Directory> { canonMakerNoteDirectory };

		// Act
		var result = ReadMetaExif.GetIsoSpeedValue(allExifItems);

		// Assert
		Assert.AreEqual(400, result);
	}

	[TestMethod]
	public void GetIsoSpeedValue_FromCanonMakerNote_AutoIso_ReturnsCalculatedIsoSpeed()
	{
		// Arrange
		var canonMakerNoteDirectory = new CanonMakernoteDirectory();

		//						15 is magic number for auto
		canonMakerNoteDirectory.Set(CanonMakernoteDirectory.CameraSettings.TagIso, "15");
		canonMakerNoteDirectory.Set(CanonMakernoteDirectory.ShotInfo.TagAutoIso, "200");
		canonMakerNoteDirectory.Set(CanonMakernoteDirectory.ShotInfo.TagBaseIso, "400");

		var allExifItems = new List<Directory> { canonMakerNoteDirectory };

		// Act
		var result = ReadMetaExif.GetIsoSpeedValue(allExifItems);

		// Assert
		Assert.AreEqual(800, result); // 400 * 200 / 100 = 800
	}

	[TestMethod]
	public void GetIsoSpeedValue_FromXmp_ReturnsIsoSpeed()
	{
		// Arrange
		var xmpStream = new MemoryStream(CreateAnImageA6600.Bytes.ToArray());

		var allExifItems = ImageMetadataReader.ReadMetadata(xmpStream).ToList();

		// Act
		var result = ReadMetaExif.GetIsoSpeedValue(allExifItems);

		// Assert
		Assert.AreEqual(800, result);

		xmpStream.Dispose();
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void ParseExifDirectory_NullItem()
	{
		var readMetaExif = new ReadMetaExif(new FakeIStorage(), new AppSettings(),
			new FakeIWebLogger());
		readMetaExif.ParseExifDirectory([], null);
		// expect exception
	}
}
