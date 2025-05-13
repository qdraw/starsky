using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif.Makernotes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace starskytest.starsky.foundation.thumbnailmeta.ServicesPreviewSize;

[TestClass]
public class Test
{
	[TestMethod]
	public void TestThumbnailSize()
	{
		var outputPath = "/tmp/preview";
		var inputPath =
			"/Users/dion/data/fotobieb/2024/11/2024_11_11_d glow eindhoven/20241111_191338_DSC01021.jpg";
		var inputPath2 =
			"/Users/dion/data/fotobieb/2024/11/2024_11_11_d glow eindhoven/20241111_191349_d.jpg";
		var directories = ImageMetadataReader.ReadMetadata(inputPath);

		var sonyDir = directories.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();
		var json =
			JsonSerializer.Serialize(sonyDir, new JsonSerializerOptions { WriteIndented = true });


		if ( sonyDir != null &&
		     sonyDir.TryGetInt32(SonyType1MakernoteDirectory.TagPreviewImage, out var offset) &&
		     sonyDir.TryGetInt32(SonyType1MakernoteDirectory.TagPreviewImageSize, out var length) )
		{
			Console.WriteLine($"📍 Found preview at offset {offset}, length {length}");

			var previewData = new byte[length];
			using ( var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read) )
			{
				fs.Seek(offset, SeekOrigin.Begin);
				var bytesRead = fs.Read(previewData, 0, length);
				if ( bytesRead != length )
				{
					Console.WriteLine("⚠️ Warning: read less data than expected.");
				}
			}

			// Optioneel: check of het een JPEG is
			if ( previewData.Length > 3 && previewData[0] == 0xFF && previewData[1] == 0xD8 &&
			     previewData[2] == 0xFF )
			{
				File.WriteAllBytes(outputPath, previewData);
				Console.WriteLine($"✅ Preview extracted to {outputPath}");
			}
			else
			{
				Console.WriteLine("❌ Preview data is not a valid JPEG.");
			}
		}
		else
		{
			Console.WriteLine("❌ Preview tags not found in Sony makernote.");
		}

		// int counter = 0;
		//
		// foreach ( var directory in directories )
		// {
		// 	foreach (var tag in directory.Tags)
		// 	{
		// 		var obj = directory.GetObject(tag.Type);
		// 		if (obj is byte[] data && data.Length > 10) // arbitrary threshold
		// 		{
		// 			try
		// 			{
		// 				
		// 				using var ms = new MemoryStream(data);
		// 				string dirName = directory.GetType().Name.Replace("Directory", "");
		// 				string outputPath = Path.Combine(outputDir, $"preview_{counter:X}_{dirName}_0x{tag.Type:X}.jpg");
		// 				
		// 				File.WriteAllBytes(outputPath, data);
		// 				
		// 				// using var image = Image.Load(ms);
		// 				// image.Save(outputPath);
		// 				Console.WriteLine($"✅ Saved preview from tag 0x{tag.Type:X} ({tag.Name}) to {outputPath}");
		// 				counter++;
		// 			}
		// 			catch
		// 			{
		// 				// not a valid image, skip
		// 				Console.WriteLine($"❌ Tag 0x{tag.Type:X} is not a valid image.");
		// 			}
		// 		}
		// 	}
		// }
		//
		// if (counter == 0)
		// {
		// 	Console.WriteLine("No valid preview images extracted.");
		// }

		// var sony = directories.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();
		// if (sony != null)
		// {
		// 	foreach (var tag in sony.Tags)
		// 	{
		// 		if (sony.GetObject(tag.Type) is byte[] bytes)
		// 		{
		// 			Console.WriteLine($"Tag 0x{tag.Type:X} ({tag.Name}) — {bytes.Length} bytes");
		// 		}
		// 	}
		// }
		//
		//
		// byte[]? previewBytes = null;
		// foreach ( var directory in directories )
		// {
		// 	var customPreview = directory.GetByteArray(0x2001);
		// 	if (customPreview?.Length >= 10)
		// 	{
		// 		previewBytes = customPreview;
		// 		break;
		// 	}
		// }
		//
		// Console.WriteLine(previewBytes);

		// byte[]? previewBytes = null;
		//
		// // 1. Try standard thumbnail (often works)
		// var sonyDir = directories
		// 	              .OfType<SonyType6MakernoteDirectory>()
		// 	              .FirstOrDefault()
		//               ?? directories.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();
		//
		// // 2. Try custom preview tag (0x2001 = 8193)
		// if (previewBytes == null)
		// {
		// 	var exifDirs = directories.OfType<ExifSubIfdDirectory>();
		// 	foreach (var dir in exifDirs)
		// 	{
		// 		var customPreview = dir.GetByteArray(0x2001);
		// 		if (customPreview != null)
		// 		{
		// 			previewBytes = customPreview;
		// 			break;
		// 		}
		// 	}
		// }
		//
		// if (previewBytes == null)
		// {
		// 	Console.WriteLine("No embedded preview image found.");
		// 	return;
		// }
		//
		// try
		// {
		// 	using var ms = new MemoryStream(previewBytes);
		// 	using var image = Image.Load<Rgba32>(ms);
		// 	image.Save(outputPath, new JpegEncoder());
		// 	Console.WriteLine($"Preview image saved to {outputPath}");
		// }
		// catch (Exception e)
		// {
		// 	Console.WriteLine($"Failed to decode preview image: {e.Message}");
		// }

		throw new Exception("test");
	}
}
