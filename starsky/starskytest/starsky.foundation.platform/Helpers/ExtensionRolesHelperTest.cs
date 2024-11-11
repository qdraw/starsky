using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageCorrupt;
using starskytest.FakeCreateAn.CreateAnImagePsd;
using starskytest.FakeCreateAn.CreateAnImageWebP;

namespace starskytest.starsky.foundation.platform.Helpers;

[TestClass]
public sealed class ExtensionRolesHelperTest
{
	[TestMethod]
	public void Files_ExtensionThumbSupportedList_TiffMp4MovXMPCheck()
	{
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionThumbnailSupported("file.tiff"));
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionThumbnailSupported("file.mp4"));
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionThumbnailSupported("file.mov"));
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionThumbnailSupported("file.xmp"));
	}

	[TestMethod]
	public void Files_ExtensionThumbSupportedList_JpgCheck()
	{
		Assert.IsTrue(ExtensionRolesHelper.IsExtensionThumbnailSupported("file.jpg"));
		Assert.IsTrue(ExtensionRolesHelper.IsExtensionThumbnailSupported("file.bmp"));
	}

	[TestMethod]
	public void Files_ExtensionThumbSupportedList_null()
	{
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionThumbnailSupported(null));
		// equal or less then three chars
		Assert.IsFalse(ExtensionRolesHelper.IsExtensionThumbnailSupported("nul"));
	}

	[TestMethod]
	public void Files_ExtensionThumbSupportedList_FolderName()
	{
		Assert.IsFalse(
			ExtensionRolesHelper.IsExtensionThumbnailSupported("Some Foldername"));
	}

	[TestMethod]
	public void Files_ExtensionSyncSupportedList_TiffCheck()
	{
		var extensionSyncSupportedList = ExtensionRolesHelper.ExtensionSyncSupportedList;
		Assert.IsTrue(extensionSyncSupportedList.Contains("tiff"));
		Assert.IsTrue(extensionSyncSupportedList.Contains("jpg"));
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
		var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 73, 73, 82 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Tiff_fujiFilmRaw()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 70, 85, 74 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Tiff_panasonicRaw()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 73, 73, 85, 0 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_Tiff_4D_4D_00_2A_big_endian()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			ExtensionRolesHelper.HexStringToByteArray("4D 4D 00 2A".Replace(" ", "")));
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
		var metaJson = new byte[]
		{
			123, 10, 32, 32, 34, 36, 105, 100, 34, 58, 32, 34, 104, 116, 116, 112, 115, 58, 47,
			47, 100, 111, 99, 115, 46, 113, 100, 114, 97, 119, 46, 110, 108, 47, 115, 99, 104,
			101, 109, 97, 47, 109, 101, 116, 97, 45, 100, 97, 116, 97, 45, 99, 111, 110, 116,
			97, 105, 110, 101, 114, 46, 106, 115, 111, 110, 34, 44
		};

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
		var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 77, 77, 42 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_tiff3_Test()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(new byte[] { 77, 77, 0 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.tiff, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_bmp_Test()
	{
		var bmBytes = Encoding.ASCII.GetBytes("BM");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.bmp, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gif_Test()
	{
		var bmBytes = Encoding.ASCII.GetBytes("GIF");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gif, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_xmp_Test()
	{
		var bmBytes = Encoding.ASCII.GetBytes("<x:xmpmeta");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.xmp, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_xmp2_Test()
	{
		var bmBytes = Encoding.ASCII.GetBytes("<?xpacket");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.xmp, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_57()
	{
		var bmBytes = Encoding.ASCII.GetBytes(
			"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\r\n<gpx creator");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_56()
	{
		var bmBytes = Encoding.ASCII.GetBytes(
			"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\n<gpx creator");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_60()
	{
		// the offset is 60 before the gpx tag
		var bmBytes = Encoding.ASCII.GetBytes(
			"   <?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\r\n<gpx creator");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Test_1()
	{
		// there is one space offset
		var bmBytes = Encoding.ASCII.GetBytes(" <gpx creator");
		var fileType = ExtensionRolesHelper.GetImageFormat(bmBytes);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, fileType);
	}

	[TestMethod]
	public void Files_GetImageFormat_gpx_Xml_Test_21()
	{
		// there is 21 offset
		var bmBytes =
			Encoding.ASCII.GetBytes(
				"<?xml version=\"1.0\"?><gpx version=\"1.0\" creator=\"Trails 1.05");
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
			Encoding.ASCII.GetBytes("                                       <gpx creator");
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
	public void Files_GetImageFormat_h264_Test()
	{
		var fileType = ExtensionRolesHelper.GetImageFormat(
			new byte[] { 00, 00, 00, 20, 102, 116, 121, 112 });
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.mp4, fileType);
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
		var gpxExample = Encoding.ASCII.GetBytes(
			"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><gpx version=\"1.1\" creator=\"Trails 4.06 - https://www.trails.io\"");

		var result = ExtensionRolesHelper.GetImageFormat(gpxExample);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, result);
	}

	[TestMethod]
	public void Gpx_Stream_WithXmlPrefix()
	{
		var gpxExample = Encoding.ASCII.GetBytes(
			"<?xml version=\"1.0\" encoding=\"UTF-8\" ?><gpx version=\"1.1\" creator=\"Trails 4.06 - https://www.trails.io\"");
		var ms = new MemoryStream(gpxExample);
		var result = ExtensionRolesHelper.GetImageFormat(ms);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.gpx, result);
	}

	[TestMethod]
	public void Gpx_WithXmlNoPrefix()
	{
		var gpxExample = Encoding.ASCII.GetBytes(
			"<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:gpxx=\"h");

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
			ExtensionRolesHelper.GetImageFormat(Stream.Null));
	}
}
