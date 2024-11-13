using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Helpers;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.thumbnailmeta.ServicesPreviewSize.Helpers;
using starsky.foundation.thumbnailmeta.ServicesPreviewSize.Interfaces;
using Directory = MetadataExtractor.Directory;
using File = TagLib.File;

namespace starsky.foundation.thumbnailmeta.ServicesPreviewSize;

public class OffsetDataMetaExifPreviewThumbnail : IOffsetDataMetaExifPreviewThumbnail
{
	private readonly IStorage _iStorage;
	private readonly IWebLogger _logger;

	public OffsetDataMetaExifPreviewThumbnail(ISelectorStorage selectorStorage, IWebLogger logger)
	{
		_iStorage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_logger = logger;
	}

	public OffsetModel ParseOffsetData(List<Directory> allExifItems, string subPath)
	{
		var json = JsonSerializer.Serialize(allExifItems);


		var (offsetSuccess, offset, byteSizeSuccess, byteSize) =
			GetOffsetAndByteSize(allExifItems, subPath);

		if ( !offsetSuccess || !byteSizeSuccess )
		{
			return new OffsetModel
			{
				Success = false,
				Reason = $"{FilenamesHelper.GetFileName(subPath)} offset or byteSize failed"
			};
		}

		var thumbnail = new byte[byteSize];

		using ( var imageStream = _iStorage.ReadStream(subPath) )
		{
			imageStream.Seek(offset, SeekOrigin.Begin);

			var actualRead = imageStream.Read(thumbnail, 0, byteSize);
			if ( actualRead != byteSize )
			{
				_logger.LogError("[ParseOffsetData] ReadStream: actualRead != maxRead");
			}
		}

		return new OffsetModel
		{
			Index = offset,
			Count = byteSize,
			Success = offsetSuccess && byteSizeSuccess,
			Reason = offsetSuccess && byteSizeSuccess ? null : "offset or byteSize failed",
			Data = thumbnail
		};
	}

	public List<Directory> ReadExifMetaDirectory(string subPath)
	{
		using var stream = _iStorage.ReadStream(subPath);
		var allExifItems =
			ImageMetadataReader.ReadMetadata(stream).ToList();
		return allExifItems;
	}

	private (bool offsetSuccess, int offset, bool byteSizeSuccess, int byteSize)
		GetOffsetAndByteSize(List<Directory> allExifItems, string subPath)
	{
		for ( var i = 0; i < 2; i++ )
		{
			var (offsetSuccess, offset, byteSizeSuccess, byteSize) = i switch
			{
				0 => GetOffsetAndByteSizeForRaw(allExifItems),
				1 => GetOffsetAndByteSizeForJpeg(subPath),
				_ => throw new ArgumentOutOfRangeException()
			};
			if ( offsetSuccess && byteSizeSuccess )
			{
				return ( offsetSuccess, offset, byteSizeSuccess, byteSize );
			}
		}

		return ( false, 0, false, 0 );
	}

	private (bool offsetSuccess, int offset, bool byteSizeSuccess, int byteSize)
		GetOffsetAndByteSizeForJpeg(string subPath)
	{
		var directories = ImageMetadataReader.ReadMetadata(
			"/Users/dion/data/fotobieb/2024/11/2024_11_11_d/20241111_181721_DSC00782.jpg");
		var directory = directories.OfType<SonyType1MakernoteDirectory>().FirstOrDefault();

		SonyMakerNotesParser.ParseSonyMakerNotes(
			"/Users/dion/data/fotobieb/2024/11/2024_11_11_d/20241111_181721_DSC00782.jpg");

		return ( false, 0, false, 0 );

		// Check if directory contains PreviewImage tag (0x2001)
		var previewImageTag = directory.GetObject(0x2001);
		if ( previewImageTag is byte[] previewImageData )
		{
			System.IO.File.WriteAllBytes("/tmp/sony_preview_image.jpg", previewImageData);
			Console.WriteLine(
				"Sony preview image extracted successfully to 'sony_preview_image.jpg'.");
		}
		else
		{
			Console.WriteLine("Preview image tag not found in Sony MakerNotes.");
		}

		using ( var image = Image.Load(_iStorage.ReadStream(subPath)) )
		{
			var exifProfile = image.Metadata.ExifProfile;
			if ( exifProfile != null )
			{
				var makerNote = exifProfile.Values.FirstOrDefault(p => p.Tag == ExifTag.MakerNote)
					?.GetValue() as byte[];
				var str = Encoding.Default.GetString(makerNote);
				// Access Exif tags
				Console.WriteLine();
			}
		}


		var file =
			File.Create(new TagLibSharpAbstractions.FileBytesAbstraction("test.jpg",
				_iStorage.ReadStream(subPath)));
		var t = file.Tag;
		var json = JsonSerializer.Serialize(t,
			new JsonSerializerOptions
			{
				WriteIndented = true,
				NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
			});

		// var test = new PreviewImageExtractor().GetImageSize(
		// 	"/Users/dion/data/git/starsky/starsky/starskytest/FakeCreateAn/CreateAnImageLargePreview/20241112_110839_DSC02741.jpg");

		var test = new PreviewImageExtractor().ExtractTagData(
			"/Users/dion/data/git/starsky/starsky/starskytest/FakeCreateAn/CreateAnImageLargePreview/20241112_110839_DSC02741.jpg",
			0x2001);

		//var t = PreviewImageExtractor.ExtractPreviewImage(_iStorage.ReadStream(subPath));

		// PreviewImageExtractor.ExtractPreviewImage(new MemoryStream())
		// var sonyMakerNote = allExifItems.OfType<SonyType1MakernoteDirectory>().ToList();
		// var preview = sonyMakerNote.FirstOrDefault()?.Tags.FirstOrDefault(p => p.Type == SonyType1MakernoteDirectory.TagPreviewImageSize)?.Description;
		// var preview1 = sonyMakerNote.FirstOrDefault()?.Tags.FirstOrDefault(p => p.Type == SonyType1MakernoteDirectory.TagPreviewImage)?.Description;
		// var test = sonyMakerNote.FirstOrDefault()?.Tags.(0x2001);
		//
		return ( false, 0, false, 0 );
	}

	private static (bool offsetSuccess, int offset, bool byteSizeSuccess, int byteSize)
		GetOffsetAndByteSizeForRaw(List<Directory> allExifItems)
	{
		var exifIfd0Directories = allExifItems.OfType<ExifIfd0Directory>().ToList();
		var exifIfd0DirectoriesTags = exifIfd0Directories.FirstOrDefault()?.Tags;
		var isCompression = exifIfd0DirectoriesTags?
			.FirstOrDefault(p => p.Type == ExifDirectoryBase.TagCompression)
			?.Description;
		if ( exifIfd0DirectoriesTags == null || isCompression == null ||
		     !isCompression.Contains("JPEG") )
		{
			return ( false, 0, false, 0 );
		}

		// you can get offset of thumbnail by JpegIFOffset(0x0201) Tag in IFD1,
		// size of thumbnail by JpegIFByteCount(0x0202) 
		// https://www.media.mit.edu/pia/Research/deepview/exif.html

		// Unknown tag (0x0201) - 135330
		var offsetAsString = exifIfd0DirectoriesTags.FirstOrDefault(p => p.Type == 513)
			?.Description;
		var offsetSuccess = int.TryParse(offsetAsString, out var offset);

		// Unknown tag (0x0202) - 840155
		var byteSizeAsString = exifIfd0DirectoriesTags.FirstOrDefault(p => p.Type == 514)
			?.Description;
		var byteSizeSuccess = int.TryParse(byteSizeAsString, out var byteSize);

		return ( offsetSuccess, offset, byteSizeSuccess, byteSize );
	}
}
