using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageA6600Raw;
using starskytest.FakeCreateAn.CreateAnImageCorrupt;
using starskytest.FakeCreateAn.CreateAnImagePsd;
using starskytest.FakeCreateAn.CreateAnImageWebP;
using starskytest.FakeCreateAn.CreateAnQuickTimeMp4;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class ExtensionRolesHelperTest
{
	[TestMethod]
	public void Files_ExtensionThumbSupportedList_TiffMp4MovXMPCheck()
	{
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported("file.tiff"));
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported("file.mp4"));
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported("file.mov"));
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported("file.xmp"));
	}

	[TestMethod]
	[DataRow("file.mp4")]
	[DataRow("file.mov")]
	[DataRow("file.mts")]
	public void Files_IsExtensionVideoSupported_VideoAndNotImages(string filePath)
	{
		// Check if Video is supported and NOT Image
		Assert.IsTrue(ExtensionRolesHelper.IsExtensionVideoSupported(filePath));
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(filePath));
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("file.txt")]
	public void Files_IsExtensionVideoSupported_NeverFound(string filePath)
	{
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionVideoSupported(filePath));
	}

	[TestMethod]
	public void Files_ExtensionThumbSupportedList_JpgCheck()
	{
		Assert.IsTrue(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported("file.jpg"));
		Assert.IsTrue(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported("file.bmp"));
	}

	[TestMethod]
	public void Files_ExtensionThumbSupportedList_null()
	{
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported(null));
		// equal or less then three chars
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported("nul"));
	}

	[TestMethod]
	public void Files_ExtensionThumbSupportedList_FolderName()
	{
		Assert.IsFalse(
			ExtensionRolesHelper.IsExtensionImageSharpThumbnailSupported("Some Foldername"));
	}

	[TestMethod]
	public void Files_ExtensionSyncSupportedList_TiffCheck()
	{
		var extensionSyncSupportedList = ExtensionRolesHelper.ExtensionSyncSupportedList;
		Assert.Contains("tiff", extensionSyncSupportedList);
		Assert.Contains("jpg", extensionSyncSupportedList);
	}

	[TestMethod]
	public void Files_GetImageFormat_png_Test()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 137, 80, 78, 71 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.png, fileType);
	}

	[TestMethod]
	public void GetImageFormat_Png_Test()
	{
		var newImage = CreateAnPng.Bytes.Take(15).ToArray();
		var result = ExtensionRolesHelper.GetImageFormat(newImage);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.png, result);
	}

	[TestMethod]
	public void Files_GetImageFormat_jpeg2_Test()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 255, 216, 255, 225 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, fileType);
	}


	[TestMethod]
	public void Files_GetImageFormat_jpeg_FF_D8_FF_DB_Test()
	{
		var fileType =
			ExtensionRolesHelper.GetImageFormat(
				ExtensionRolesHelper.HexStringToByteArray("FFD8FFDB"));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_jpeg_FF_D8_FF_E0_00_10_Test()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(
				"FF D8 FF E0 00 10 4A 46 49 46 00 01".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_jpeg_FF_D8_FF_EE_Test()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("FF D8 FF EE ".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_jpeg_FF_D8_FF_E1___45_78_Test()
	{
		// FF D8 FF E1 ?? ?? 45 78
		// 69 66 00 00
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("FF D8 FF E1".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_jpeg_FF_D8_FF_E0_Test()
	{
		// FF D8 FF E0 
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("FF D8 FF E0 ".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_png_89_50_4E_47_0D_0A_1A_0A()
	{
		// 89 50 4E 47 0D 0A 1A 0A
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(
				"89 50 4E 47 0D 0A 1A 0A".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.png, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_pdf()
	{
		// 25 50 44 46 2D 
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("25 50 44 46 2D".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.pdf, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Mpeg4_66_74_79_70_69_73_6F_6D()
	{
		// 66 74 79 70 69 73 6F 6D 
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(
				"66 74 79 70 69 73 6F 6D".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Cr3_IsobmffFtypCrxBrand_ReturnsCr3()
	{
		// [size:4][ftyp][crx ]
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(
				"00 00 00 18 66 74 79 70 63 72 78 20".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.cr3, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Mp4_IsobmffFtypIsomAtOffset4_ReturnsMp4()
	{
		// [size:4][ftyp][isom]
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(
				"00 00 00 18 66 74 79 70 69 73 6F 6D".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, fileType);
	}

	[TestMethod]
	[DataRow("47 40 11 10 00 42 F0 25 00 01 C1 00 00 FF 01 FF 00 01 FC 80 14 " +
	         "48 12 01 06 46 46 6D 70 65 67 09 53 65 72 76 69 63 65 30 31 77 7C 43 CA",
		ExtensionRolesHelper.ImageFormat.mts,
		DisplayName = "Valid MTS Pattern 1")]
	[DataRow("00 00 00 00 47 40 00 10 00 00 B0 11 00 00 C1 00 00 00 00 E0 1F " +
	         "00 01 E1 00 23 5A AB 82 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF",
		ExtensionRolesHelper.ImageFormat.mts,
		DisplayName = "Valid MTS Pattern 2")]
	[DataRow("47 40 11 10 00 42 F0 25 00 01 C1 00 00",
		ExtensionRolesHelper.ImageFormat.unknown,
		DisplayName = "To short for MTS")]
	[DataRow("47 5F FF 10 00 42 F0 25 00 01 C1 00 00 FF 01 FF 00 01 FC 80 14 " +
	         "48 12 01 06 46 46 6D 70 65 67 09 53 65 72 76 69 63 65 30 31 77 7C 43 CA",
		ExtensionRolesHelper.ImageFormat.unknown,
		DisplayName = "Invalid PID (0x1FFF)")]
	[DataRow("47 40 11 00 00 42 F0 25 00 01 C1 00 00 FF 01 FF 00 01 FC 80 14 " +
	         "48 12 01 06 46 46 6D 70 65 67 09 53 65 72 76 69 63 65 30 31 77 7C 43 CA",
		ExtensionRolesHelper.ImageFormat.unknown,
		DisplayName = "Adaptation field control = 0 (invalid)")]
	[DataRow("00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 " +
	         "14 15 16 17 18 19 1A 1B 1C 1D 1E 1F 20 21 22 23 24 25 26 27 28 29 2A 2B",
		ExtensionRolesHelper.ImageFormat.unknown,
		DisplayName = "No sync byte at offset 0 or 4")]
	[DataRow("47 40 11",
		ExtensionRolesHelper.ImageFormat.unknown,
		DisplayName = "Not enough bytes for header fields")]
	public void GetImageFormat_Mts(string hexValue, ExtensionRolesHelper.ImageFormat expected)
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(hexValue.Replace(" ", "")));
		Assert.AreEqual(expected, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Tiff_49_49_2A_00_little_endian()
	{
		// 49_49_2A_00_little_endian
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("49 49 2A 00".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Tiff_olympusRaw()
	{
		// Provide proper little-endian TIFF header with Olympus marker
		var bytes = "II*\0"u8.ToArray();
		var fullBytes = bytes.Concat("OLYMP"u8.ToArray()).ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(fullBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.orf, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Tiff_fujiFilmRaw()
	{
		// Fuji RAF files start with FUJI marker (not standard TIFF header at start)
		var fileType =
			ExtensionRolesHelper.GetImageFormat("FUJI"u8.ToArray()); // FUJI
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.raf, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Tiff_panasonicRaw()
	{
		// Provide proper little-endian TIFF header with Panasonic marker
		var bytes = "II*\0"u8.ToArray();
		var fullBytes = bytes.Concat("Panasonic"u8.ToArray()).ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(fullBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.rw2, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Tiff_4D_4D_00_2A_big_endian()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("4D 4D 00 2A".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}


	[TestMethod]
	public void Files_GetImageFormat_SonyArw_WithTiffHeader()
	{
		// Sony ARW files have little-endian TIFF header + SONY marker
		var bytes = "II*\0"u8.ToArray();
		var fullBytes = bytes.Concat("SONY"u8.ToArray()).ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(fullBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.arw, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_SonyArw1_WithTiffHeader()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			[.. new CreateAnImageA6600Raw().Bytes]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.arw, fileType);
	}


	[TestMethod]
	public void Files_CreateAnQuickTimeMp4A6700()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			[.. new CreateAnQuickTimeMp4A6700().Bytes]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_NikonNef_WithTiffHeader()
	{
		// Nikon NEF files have TIFF header + NIKON marker
		var bytes = "MM\0*"u8.ToArray(); // Big-endian TIFF
		var fullBytes = bytes.Concat("NIKON"u8.ToArray()).ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(fullBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.nef, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_PentaxPef_WithTiffHeader()
	{
		// Pentax PEF files have TIFF header + PENTAX marker
		var bytes = "MM\0*"u8.ToArray(); // Big-endian TIFF
		var fullBytes = bytes.Concat("PENTAX"u8.ToArray()).ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(fullBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.pef, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_RawMarkerAtDifferentOffsets()
	{
		// Test that raw markers are found even when at different offsets
		var bytes = "II*\0"u8.ToArray();
		// SONY marker at offset 20
		var fullBytes = bytes
			.Concat(new byte[16])
			.Concat("SONY"u8.ToArray())
			.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(fullBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.arw, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_DefaultTiffWhenNoRawMarker()
	{
		// TIFF header without any raw format marker should return TIFF
		var bytes = "II*\0"u8.ToArray();
		var fullBytes = bytes.Concat(new byte[100]).ToArray(); // Add padding without raw markers
		var fileType = ExtensionRolesHelper.GetImageFormat(fullBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}


	[TestMethod]
	public void Files_GetImageFormat_zip_50_4B_03_04()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("50 4B 03 04".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.zip, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_bmp_42_4D()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("42 4D".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.bmp, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_MetaJson_1()
	{
		var metaJson = "{\n  \"$id\": \"https://docs.qdraw.nl/schema/meta-data-container.json\","u8.ToArray();

		var fileType = ExtensionRolesHelper.GetImageFormat(metaJson);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.meta_json, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_MetaJson_Windows()
	{
		var metaJsonWindows = new byte[]
		{
			// 13 is CR
			123, 13, 10, 32, 32, 34, 36, 105, 100, 34, 58, 32, 34, 104, 116, 116, 112, 115, 58, 47,
			47, 100, 111, 99, 115, 46, 113, 100, 114, 97, 119, 46, 110, 108, 47, 115, 99, 104, 101,
			109, 97, 47, 109, 101, 116, 97, 45, 100, 97, 116, 97, 45, 99, 111, 110, 116, 97, 105,
			110, 101, 114, 46, 106, 115, 111, 110, 34
		};
		// B0D0A202022246964223A202268747470733A2F2F646F63732E71647261772E6E6C2F736368656D612F6D6574612D646174612D636F6E7461696E65722E6A736F6E22

		var fileType = ExtensionRolesHelper.GetImageFormat(metaJsonWindows);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.meta_json, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_corrupt_Test()
	{
		var fileType =
			ExtensionRolesHelper.GetImageFormat(new CreateAnImageCorrupt().Bytes.ToArray());
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, fileType);
	}

	[TestMethod]
	public void GetImageFormat_Jpeg_Test()
	{
		var newImage = CreateAnImage.Bytes.Take(15).ToArray();
		var result = ExtensionRolesHelper.GetImageFormat(newImage);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, result);
	}

	[TestMethod]
	public void Files_GetImageFormat_tiff2_Test()
	{
		// Valid little-endian TIFF header: II* (0x49 0x49 0x2A 0x00)
		var fileType = ExtensionRolesHelper.GetImageFormat("II*\0"u8.ToArray());
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_tiff3_Test()
	{
		// Valid big-endian TIFF header: MM\0* (0x4D 0x4D 0x00 0x2A)
		var fileType = ExtensionRolesHelper.GetImageFormat("MM\0*"u8.ToArray());
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_bmp_Test()
	{
		var bmBytes = "BM"u8.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.bmp, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gif_Test()
	{
		var bmBytes = "GIF"u8.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gif, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_xmp_Test()
	{
		var bmBytes = "<x:xmpmeta"u8.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.xmp, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_xmp2_Test()
	{
		var bmBytes = "<?xpacket"u8.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.xmp, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_57()
	{
		var bmBytes =
			"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\r\n<gpx creator"u8
				.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_56()
	{
		var bmBytes =
			"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\n<gpx creator"u8
				.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_60()
	{
		// the offset is 60 before the gpx tag
		var bmBytes =
			"   <?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\r\n<gpx creator"u8
				.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_58()
	{
		// the offset is 58
		var bmBytes = Encoding.ASCII.GetBytes(
			"<?xml version='1.0' encoding='UTF-8' standalone='yes' ?>\r\n" +
			"<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" " +
			"xmlns:geotracker=\"http://ilyabogdanovich.com/gpx/extensions/geotracker\" " +
			"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
			"xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 " +
			"http://www.topografix.com/GPX/1/1/gpx.xsd\" version=\"1.1\" " +
			"creator=\"Opgenomen in met Geo Tracker voor iOS van Ilya Bogdanovich\">\n");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}


	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_1()
	{
		// there is one space offset
		var bmBytes = " <gpx creator"u8.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Xml_Test_21()
	{
		// there is 21 offset
		var bmBytes =
			"<?xml version=\"1.0\"?><gpx version=\"1.0\" creator=\"Trails 1.05"u8.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Xml_Test_xxx()
	{
		const string text =
			"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>\n<gpx xmlns";
		var bmBytes = Encoding.ASCII.GetBytes(text);
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void GetImageFormat_Jpeg_Example()
	{
		// 20201005_155330_DSC05634_meta_thumb
		var jpeg4 = new byte[] { 255, 216, 255, 237 };
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg,
			ExtensionRolesHelper.GetImageFormat(jpeg4));
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_39()
	{
		// the number of spaces is 39 before <gpx creator
		var bmBytes =
			"                                       <gpx creator"u8.ToArray();
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}


	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_38()
	{
		// the offset is 38
		var calcOffsetBytes = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>"u8.ToArray();
		Assert.HasCount(38, calcOffsetBytes);

		var bmBytes = Encoding.ASCII.GetBytes(
			"<?xml version=\"1.0\" encoding=\"UTF-8\"?><gpx creator=\"" +
			"Wikiloc - https://www.wikiloc.com\" " +
			"version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\" " +
			"xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
			"xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 " +
			"http://www.topografix.com/GPX/1/1/gpx.xsd\"><metadata>" +
			"<name>Wikiloc - PR-CV 78 Els Paratges de Xàtiva.</name><author><name>");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_IsExtensionForceXmp_Positive()
	{
		var result = ExtensionRolesHelper.IsExtensionForceXmp("/test.arw");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_IsExtensionForceXmp_Null()
	{
		var result = ExtensionRolesHelper.IsExtensionForceXmp(null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_IsExtensionForceXmp_Negative()
	{
		var result = ExtensionRolesHelper.IsExtensionForceXmp("/test.jpg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_ReplaceExtensionWithXmp_arw()
	{
		var result = ExtensionRolesHelper.ReplaceExtensionWithXmp("/test.arw");
		Assert.AreEqual("/test.xmp", result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_ReplaceExtensionWithXmp_tiff()
	{
		var result = ExtensionRolesHelper.ReplaceExtensionWithXmp("/test.tiff");
		Assert.AreEqual("/test.xmp", result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_ReplaceExtensionWithXmp_fail()
	{
		var result = ExtensionRolesHelper.ReplaceExtensionWithXmp("/test.so");
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_ReplaceExtensionWithXmp_null()
	{
		var result = ExtensionRolesHelper.ReplaceExtensionWithXmp(null);
		Assert.AreEqual(string.Empty, result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_IsExtensionSidecar_Null()
	{
		var result = ExtensionRolesHelper.IsExtensionSidecar(null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_IsExtensionSidecar_Xmp()
	{
		var result = ExtensionRolesHelper.IsExtensionSidecar("test.xmp");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_IsExtensionSidecar_MetaJson()
	{
		var result = ExtensionRolesHelper.IsExtensionSidecar("test.meta.json");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_IsExtensionForceGpx_Null()
	{
		var result = ExtensionRolesHelper.IsExtensionForceGpx(null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void ExtensionRolesHelperTest_IsExtensionExifToolSupported_Null()
	{
		var result = ExtensionRolesHelper.IsExtensionExifToolSupported(null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	[DataRow("/test/sample.fff")]
	[DataRow("/test/sample.x3f")]
	public void ExtensionRolesHelperTest_IsExtensionRawThumbnailSupported_Phase3Formats(
		string filePath)
	{
		var result = ExtensionRolesHelper.IsExtensionRawThumbnailSupported(filePath);
		Assert.IsTrue(result);
	}

	[TestMethod]
	[DataRow("/test/sample.jpg")]
	[DataRow("/test/sample.mp3")]
	public void ExtensionRolesHelperTest_IsExtensionRawThumbnailSupported_NonRawFormats(
		string filePath)
	{
		var result = ExtensionRolesHelper.IsExtensionRawThumbnailSupported(filePath);
		Assert.IsFalse(result);
	}

	[TestMethod]
	[DataRow("/test/sample.fff")]
	[DataRow("/test/sample.x3f")]
	public void ExtensionRolesHelperTest_IsExtensionForceXmp_Phase3Formats(string filePath)
	{
		var result = ExtensionRolesHelper.IsExtensionForceXmp(filePath);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void Files_GetImageFormat_h264_Test()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			new byte[] { 00, 00, 00, 20, 102, 116, 121, 112 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_mjpeg_Test()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
		[
			0, 0, 0, 20, 112, 110, 111, 116, 190, 79, 137, 23,
			0, 0, 80, 73, 67, 84, 0, 1, 0, 0, 31, 132, 80, 73
		]);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mjpeg, fileType);
	}

	[TestMethod]
	public void GetImageFormat_QuickTimeMp4_Test()
	{
		var newImage = CreateAnQuickTimeMp4.Bytes.Take(15).ToArray();
		var result = ExtensionRolesHelper.GetImageFormat(newImage);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, result);
	}

	[TestMethod]
	public void Gpx_WithXmlPrefix()
	{
		var gpxExample =
			"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><gpx version=\"1.1\" creator=\"Trails 4.06 - https://www.trails.io\""u8
				.ToArray();

		var result = ExtensionRolesHelper.GetImageFormat(gpxExample);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, result);
	}

	[TestMethod]
	public void Gpx_Stream_WithXmlPrefix()
	{
		var gpxExample = Encoding.ASCII.GetBytes(
			"<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
			"<gpx version=\"1.1\" creator=\"Trails 4.06 - https://www.trails.io\"");
		var ms = new MemoryStream(gpxExample);
		var result = new ExtensionRolesHelper(new FakeIWebLogger()).GetImageFormat(ms);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, result);
	}

	[TestMethod]
	public void GetImageFormat_Stream_Uses400ByteProbe_ForRawDetection()
	{
		var bytes = new byte[400];
		bytes[0] = 0x49;
		bytes[1] = 0x49;
		bytes[2] = 0x2A;
		bytes[3] = 0x00;
		bytes[4] = 8;
		bytes[5] = 0;
		bytes[6] = 0;
		bytes[7] = 0;
		Encoding.ASCII.GetBytes("SONY", 0, 4, bytes, 350);

		var ms = new MemoryStream(bytes);
		var result = new ExtensionRolesHelper(new FakeIWebLogger()).GetImageFormat(ms);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.arw, result);
	}

	[TestMethod]
	public void Gpx_WithXmlNoPrefix()
	{
		var gpxExample =
			"<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:gpxx=\"h"u8.ToArray();

		var result = ExtensionRolesHelper.GetImageFormat(gpxExample);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, result);
	}

	[TestMethod]
	public void Gpx_CreateAnGpx()
	{
		var result = ExtensionRolesHelper.GetImageFormat(CreateAnGpx.Bytes.ToArray());
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, result);
	}

	[TestMethod]
	public void WebP_CreateAnWebP()
	{
		var createAnImage = new CreateAnImageWebP().Bytes.ToArray();
		var result = ExtensionRolesHelper.GetImageFormat(createAnImage);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.webp, result);
	}

	[TestMethod]
	public void Files_GetImageFormat_WebpHex()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(
				"52 49 46 46 00 00 00 00 57 45 42 50 56 50 38".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.webp, fileType);
	}

	[TestMethod]
	[DataRow(
		"52 49 46 46 00 00 00 00 51 4C 43 4D")] // Qualcomm PureVoice file format (QCP) looks like webp
	[DataRow("52 49 46 46 00 00 00 00 43 44 44 41")] // .cda file format looks like webp
	public void UnknownFileFormat_Hex(string hexValue)
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(hexValue.Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, fileType);
	}

	[TestMethod]
	public void Psd_CreateAnPsd()
	{
		var createAnImage = new CreateAnImagePsd().Bytes.ToArray();
		var result = ExtensionRolesHelper.GetImageFormat(createAnImage);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.psd, result);
	}

	[TestMethod]
	public void Files_GetImageFormat_PsdHex()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray(
				"38 42 50 53".Replace(" ", "")));
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.psd, fileType);
	}

	[TestMethod]
	public void StringToByteArrayTest()
	{
		Assert.AreEqual(119, ExtensionRolesHelper.HexStringToByteArray("77")[0]);
	}

	[TestMethod]
	public void MapFileTypesToExtension_fileWithNoExtension()
	{
		var result = ExtensionRolesHelper.MapFileTypesToExtension("no_ext");
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void MapFileTypesToExtension_fileWithNonExistingExtension()
	{
		var result = ExtensionRolesHelper.MapFileTypesToExtension("non.ext");
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void MapFileTypesToExtension_fileWithNonExistingExtension2()
	{
		var result = ExtensionRolesHelper.MapFileTypesToExtension("non.xxx");
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void MapFileTypesToExtension_example()
	{
		var result = ExtensionRolesHelper.MapFileTypesToExtension("non.jpeg");
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, result);
	}

	[TestMethod]
	public void MapFileTypesToExtension_Null()
	{
		var result = ExtensionRolesHelper.MapFileTypesToExtension(null!);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, result);
	}

	[TestMethod]
	public void IsExtensionExifToolSupported_Null()
	{
		var result = ExtensionRolesHelper.IsExtensionExifToolSupported(null!);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsExtensionExifToolSupported_fileWithNoExtension()
	{
		var result = ExtensionRolesHelper.IsExtensionExifToolSupported("no_ext");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void GetImageFormat_NotFound()
	{
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.notfound,
			new ExtensionRolesHelper(new FakeIWebLogger()).GetImageFormat(Stream.Null));
	}
}
