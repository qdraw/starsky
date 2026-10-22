using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Bmp;
using MetadataExtractor.Formats.Exif.Makernotes;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Xmp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using XmpCore;
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
	public void GetIsoSpeedValue_CreateAnImageA6600_FromXmp_ReturnsIsoSpeed()
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
	public void ParseExifDirectory_NullItem()
	{
		var readMetaExif =
			new ReadMetaExif(new FakeIStorage(), new AppSettings(), new FakeIWebLogger());

		Assert.ThrowsExactly<ArgumentException>(() =>
			readMetaExif.ParseExifDirectory([], null)
		);
	}

	[TestMethod]
	public void ParseExifDirectory_WithExampleData_ShouldReturnExpectedFileIndexItem()
	{
		// Arrange
		var exifIfd0Directory = new ExifIfd0Directory();
		exifIfd0Directory.Set(ExifDirectoryBase.TagMake, "Canon");
		exifIfd0Directory.Set(ExifDirectoryBase.TagModel, "EOS 80D");
		exifIfd0Directory.Set(ExifDirectoryBase.TagOrientation,
			"Top, left side (Horizontal / normal)");

		// Yes this one has duplicate models, that how sony stores it
		var exifIfd0Directory2 = new ExifSubIfdDirectory();
		exifIfd0Directory2.Set(ExifDirectoryBase.TagLensSpecification, "18-200mm f/3,5-6,3");
		exifIfd0Directory2.Set(ExifDirectoryBase.TagLensModel, "E 18-200mm F3.5-6.3 OSS LE");

		var exifSubIfdDirectory = new ExifSubIfdDirectory();
		exifSubIfdDirectory.Set(ExifDirectoryBase.TagFocalLength, "50 mm");

		var directories =
			new List<Directory> { exifIfd0Directory, exifSubIfdDirectory, exifIfd0Directory2 };

		var fileIndexItem = new FileIndexItem("/path/to/file.jpg");

		var readMetaExif = new ReadMetaExif(null!, null!, null!);

		// Act
		var result = readMetaExif.ParseExifDirectory(directories, fileIndexItem);

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual("Canon", result.Make);
		Assert.AreEqual("EOS 80D", result.Model);
		Assert.AreEqual(ImageRotation.Rotation.Horizontal, result.Orientation);
		Assert.AreEqual("E 18-200mm F3.5-6.3 OSS LE", result.LensModel);
	}

	[TestMethod]
	public void ParseExifDirectory_ReadsAiFieldsFromXmpDirectory()
	{
		const string xmpData = "<x:xmpmeta xmlns:x='adobe:ns:meta/'><rdf:RDF " +
		                       "xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'><rdf:Description " +
		                       "rdf:about='' xmlns:qdraw='https://qdraw.nl/ns/qdraw/1.0/'><qdraw:SuggestedTags><rdf:Bag>" +
		                       "<rdf:li>cat</rdf:li><rdf:li>park</rdf:li></rdf:Bag></qdraw:SuggestedTags>" +
		                       "<qdraw:RejectedTags><rdf:Bag><rdf:li>car</rdf:li></rdf:Bag></qdraw:RejectedTags>" +
		                       "<qdraw:ImageClassificationModel>vit-base-1</qdraw:ImageClassificationModel>" +
		                       "<qdraw:ImageClassificationGeneratedAt>2026-04-21T10:20:30Z</qdraw:ImageClassificationGeneratedAt>" +
		                       "</rdf:Description></rdf:RDF></x:xmpmeta>";

		var xmpDirectory = new XmpDirectory();
		xmpDirectory.SetXmpMeta(XmpMetaFactory.ParseFromString(xmpData));

		var directories = new List<Directory> { xmpDirectory };
		var readMetaExif =
			new ReadMetaExif(new FakeIStorage(), new AppSettings(), new FakeIWebLogger());
		var item = new FileIndexItem("/test.jpg");

		var result = readMetaExif.ParseExifDirectory(directories, item);

		Assert.AreEqual("cat, park", result.SuggestedTags);
		Assert.AreEqual("car", result.RejectedTags);
		Assert.AreEqual("vit-base-1", result.ImageClassificationModel);
		Assert.AreEqual(new DateTime(2026, 4, 21, 10, 20, 30, DateTimeKind.Utc),
			result.ImageClassificationGeneratedAt);
	}

	private static XmpDirectory CreateXmpDirectory(string attributes, string children = "")
	{
		var xmpData = "<x:xmpmeta xmlns:x='adobe:ns:meta/'><rdf:RDF " +
		              "xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>" +
		              "<rdf:Description rdf:about='' xmlns:exif='http://ns.adobe.com/exif/1.0/' " +
		              "xmlns:dc='http://purl.org/dc/elements/1.1/' " +
		              "xmlns:qdraw='https://qdraw.nl/ns/qdraw/1.0/' " +
		              $"{attributes}>{children}</rdf:Description></rdf:RDF></x:xmpmeta>";
		var xmpDirectory = new XmpDirectory();
		xmpDirectory.SetXmpMeta(XmpMetaFactory.ParseFromString(xmpData));
		return xmpDirectory;
	}

	private static FileIndexItem Parse(params Directory[] directories)
	{
		return new ReadMetaExif(new FakeIStorage(), new AppSettings(), new FakeIWebLogger())
			.ParseExifDirectory(directories.ToList(), new FileIndexItem("/test.jpg"));
	}

	[TestMethod]
	[DataRow("On", ImageStabilisationType.On)]
	[DataRow("Off", ImageStabilisationType.Off)]
	[DataRow("Other", ImageStabilisationType.Unknown)]
	public void ParseExifDirectory_XmpImageStabilization(string value,
		ImageStabilisationType expected)
	{
		var result = Parse(CreateXmpDirectory($"qdraw:ImageStabilization='{value}'"));

		Assert.AreEqual(expected, result.ImageStabilisation);
	}

	[TestMethod]
	public void ParseExifDirectory_BmpHeader_SetsImageFormat()
	{
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.bmp, Parse(new BmpHeaderDirectory()).ImageFormat);
	}

	[TestMethod]
	public void ParseExifDirectory_GifHeader_SetsImageFormat()
	{
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gif, Parse(new GifHeaderDirectory()).ImageFormat);
	}

	[TestMethod]
	public void ParseExifDirectory_ExistingLensModel_IsNotOverwrittenBySony()
	{
		var item = new FileIndexItem("/test.jpg");
		item.SetMakeModel("My Lens", 2);
		var sony = new SonyType1MakernoteDirectory();
		sony.Set(SonyType1MakernoteDirectory.TagLensId, 1);

		var result = new ReadMetaExif(new FakeIStorage(), new AppSettings(),
			new FakeIWebLogger()).ParseExifDirectory([sony], item);

		Assert.AreEqual("My Lens", result.LensModel);
	}

	[TestMethod]
	public void ParseExifDirectory_XmpDcCreator_IsUsedAsArtist()
	{
		var xmp = new XmpDirectory();
		xmp.SetXmpMeta(new XmpMeta());
		xmp.XmpMeta?.SetProperty("http://purl.org/dc/elements/1.1/", "creator", "Jane");

		var result = Parse(xmp);

		Assert.AreEqual("Jane", result.Artist);
	}

	[TestMethod]
	public void GetXmpGeoData_WithoutHemisphere_ReturnsZero()
	{
		var xmp = CreateXmpDirectory("exif:GPSLatitude='52,20.708'");

		var result = ReadMetaExif.GetXmpGeoData([xmp], "exif:GPSLatitude");

		Assert.AreEqual(0, result, 0.00001);
	}

	[TestMethod]
	public void GetImageWidthHeight_FewerItemsThanMaxCount_DoesNotThrow()
	{
		// 7 items: more than 5 (maxCount of 6/30) but less than the 30 of "Exif SubIFD"
		List<Directory> list = [.. Enumerable.Range(0, 7).Select(_ => new ExifIfd0Directory())];

		Assert.AreEqual(0, ReadMetaExif.GetImageWidthHeight(list, true));
	}

	[TestMethod]
	public void GetImageWidthHeight_NegativeWidth_ReturnsZero()
	{
		var jpeg = new JpegDirectory();
		jpeg.Set(JpegDirectory.TagImageWidth, -5);

		Assert.AreEqual(0, ReadMetaExif.GetImageWidthHeight([jpeg], true));
	}

	[TestMethod]
	public void GetImageWidthHeight_NonNumericWidth_ReturnsZero()
	{
		var jpeg = new JpegDirectory();
		jpeg.Set(JpegDirectory.TagImageWidth, "abc");

		Assert.AreEqual(0, ReadMetaExif.GetImageWidthHeight([jpeg], true));
	}

	[TestMethod]
	public void ParseExifDirectory_XmpFallbacks_ApertureShutterFocalIso()
	{
		var result = Parse(CreateXmpDirectory(
			"exif:FNumber='9/1' exif:ExposureTime='1/20' exif:FocalLength='28/1'",
			"<exif:ISOSpeedRatings><rdf:Seq xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>" +
			"<rdf:li>400</rdf:li></rdf:Seq></exif:ISOSpeedRatings>"));

		Assert.AreEqual(9d, result.Aperture, 0.0001);
		Assert.AreEqual("1/20", result.ShutterSpeed);
		Assert.AreEqual(28d, result.FocalLength, 0.0001);
		Assert.AreEqual(400, result.IsoSpeed);
	}

	[TestMethod]
	public void ParseExifDirectory_TooLongShutterSpeed_IsIgnored()
	{
		var sub = new ExifSubIfdDirectory();
		sub.Set(ExifDirectoryBase.TagExposureTime, "123456789012345678901234");

		Assert.AreEqual(string.Empty, Parse(sub).ShutterSpeed);
	}
}
