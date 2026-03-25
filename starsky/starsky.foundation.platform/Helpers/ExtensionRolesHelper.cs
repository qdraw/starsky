using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.platform.Helpers;

public partial class ExtensionRolesHelper(IWebLogger logger)
{
	/// <summary>
	///     ImageFormat based on first bytes, so read first bytes
	/// </summary>
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum ImageFormat
	{
		notfound = -1,
		unknown = 0,

		// Viewable types
		jpg = 10,
		tiff = 12,
		bmp = 13,
		gif = 14,
		png = 15,
		webp = 16,
		psd = 17,

		// raw formats
		dng = 20, // adobe
		arw = 21, // sony
		nef = 22, // nikon
		cr2 = 23, // canon
		cr3 = 24, // canon 
		raf = 25, // Fuji
		orf = 26, // olympus 
		rw2 = 27, // Panasonic
		pef = 28, // pentax
		x3f = 29, // sigma
		// fff hasselblad has tiff previews

		// documents
		gpx = 40,
		pdf = 41,

		// video
		mp4 = 50,
		mjpeg = 51,
		mts = 52,

		// archives
		zip = 60,

		// Sidecar files
		xmp = 70,

		/// <summary>
		///     Extension: .meta.json
		/// </summary>
		meta_json = 71,

		// folder
		directory = 1000
	}

	public const int ImageFormatByteSize = 400;

	/// <summary>
	///     Xmp sidecar file
	/// </summary>
	private static readonly List<string> ExtensionXmp = ["xmp"];

	/// <summary>
	///     Meta.json sidecar files
	/// </summary>
	private static readonly List<string>
		ExtensionJsonSidecar = ["meta.json"];


	/// <summary>
	///     List of .jpg,.jpeg extensions
	/// </summary>
	private static readonly List<string> ExtensionJpg = ["jpg", "jpeg"];

	/// <summary>
	///     Tiff
	/// </summary>
	private static readonly List<string> ExtensionTiff = ["tiff"];

	/// <summary>
	///     Raws
	///     arw:sony, dng:adobe, nef:nikon, raf:fuji, cr2:canon,  orf:olympus, rw2:panasonic,
	///     pef:pentax, fff:hasselblad, x3f:sigma,
	///     Not supported: crw:canon
	/// </summary>
	private static readonly List<string> ExtensionRawSony =
	[
		"arw"
	];

	private static readonly List<string> ExtensionRawAdobe =
	[
		"dng"
	];

	private static readonly List<string> ExtensionRawNef =
	[
		"nef"
	];

	private static readonly List<string> ExtensionRawRaf =
	[
		"raf"
	];

	private static readonly List<string> ExtensionRawCr2 =
	[
		"cr2"
	];

	private static readonly List<string> ExtensionRawCr3 =
	[
		"cr3"
	];

	private static readonly List<string> ExtensionRawOrf =
	[
		"orf"
	];

	private static readonly List<string> ExtensionRawRw2 =
	[
		"rw2"
	];

	private static readonly List<string> ExtensionRawPef =
	[
		"pef"
	];

	private static readonly List<string> ExtensionRawFff =
	[
		"fff"
	];

	private static readonly List<string> ExtensionRawX3F =
	[
		"x3f"
	];

	/// <summary>
	///     Bitmaps
	/// </summary>
	private static readonly List<string> ExtensionBmp = ["bmp"];

	/// <summary>
	///     GIF based images
	/// </summary>
	private static readonly List<string> ExtensionGif = ["gif"];

	/// <summary>
	///     PNG
	/// </summary>
	private static readonly List<string> ExtensionPng = ["png"];

	/// <summary>
	///     GPX, list of geolocations
	/// </summary>
	private static readonly List<string> ExtensionGpx = ["gpx"];

	/// <summary>
	///     Mp4 Videos in h264 codex / And Quicktime
	/// </summary>
	private static readonly List<string> ExtensionMp4 = ["mp4", "mov"];

	/// <summary>
	///     Video with MTS stream
	/// </summary>
	private static readonly List<string> ExtensionMts = ["mts"];

	/// <summary>
	///     MJPEG video stream
	/// </summary>
	private static readonly List<string> ExtensionMjpeg = ["mjpeg", "mov"];

	/// <summary>
	///     WebP imageFormat
	/// </summary>
	private static readonly List<string> ExtensionWebp = ["webp"];

	/// <summary>
	///     Psd imageFormat
	/// </summary>
	private static readonly List<string> ExtensionPsd = ["psd"];

	private static readonly Dictionary<ImageFormat, List<string>>
		MapFileTypesToExtensionDictionary =
			new()
			{
				{ ImageFormat.jpg, ExtensionJpg },
				{ ImageFormat.tiff, ExtensionTiff },
				{ ImageFormat.arw, ExtensionRawSony },
				{ ImageFormat.dng, ExtensionRawAdobe },
				{ ImageFormat.nef, ExtensionRawNef },
				{ ImageFormat.raf, ExtensionRawRaf },
				{ ImageFormat.cr2, ExtensionRawCr2 },
				{ ImageFormat.cr3, ExtensionRawCr3 },
				{ ImageFormat.orf, ExtensionRawOrf },
				{ ImageFormat.rw2, ExtensionRawRw2 },
				{ ImageFormat.pef, ExtensionRawPef },
				{ ImageFormat.x3f, ExtensionRawX3F },
				{ ImageFormat.bmp, ExtensionBmp },
				{ ImageFormat.gif, ExtensionGif },
				{ ImageFormat.png, ExtensionPng },
				{ ImageFormat.gpx, ExtensionGpx },
				{ ImageFormat.mp4, ExtensionMp4 },
				{ ImageFormat.mts, ExtensionMts },
				{ ImageFormat.xmp, ExtensionXmp },
				{ ImageFormat.webp, ExtensionWebp },
				{ ImageFormat.psd, ExtensionPsd }
			};

	/// <summary>
	///     Supported by sync agent
	/// </summary>
	public static List<string> ExtensionSyncSupportedList
	{
		get
		{
			var extensionList = new List<string>();
			extensionList.AddRange(ExtensionPng);
			extensionList.AddRange(ExtensionJpg);
			extensionList.AddRange(ExtensionTiff);
			extensionList.AddRange(ExtensionRawSony);
			extensionList.AddRange(ExtensionRawAdobe);
			extensionList.AddRange(ExtensionRawNef);
			extensionList.AddRange(ExtensionRawRaf);
			extensionList.AddRange(ExtensionRawCr2);
			extensionList.AddRange(ExtensionRawCr3);
			extensionList.AddRange(ExtensionRawOrf);
			extensionList.AddRange(ExtensionRawRw2);
			extensionList.AddRange(ExtensionRawPef);
			extensionList.AddRange(ExtensionRawFff);
			extensionList.AddRange(ExtensionRawX3F);
			extensionList.AddRange(ExtensionBmp);
			extensionList.AddRange(ExtensionGif);
			extensionList.AddRange(ExtensionPng);
			extensionList.AddRange(ExtensionGpx);
			extensionList.AddRange(ExtensionMp4);
			extensionList.AddRange(ExtensionMts);
			extensionList.AddRange(ExtensionMjpeg);
			extensionList.AddRange(ExtensionXmp);
			extensionList.AddRange(ExtensionJsonSidecar);
			extensionList.AddRange(ExtensionWebp);
			extensionList.AddRange(ExtensionPsd);
			return extensionList;
		}
	}

	/// <summary>
	///     List of extensions supported by exifTool
	/// </summary>
	private static List<string> ExtensionExifToolSupportedList
	{
		get
		{
			var extensionList = new List<string>();
			extensionList.AddRange(ExtensionJpg);
			extensionList.AddRange(ExtensionTiff);
			extensionList.AddRange(ExtensionBmp);
			extensionList.AddRange(ExtensionGif);
			extensionList.AddRange(ExtensionPng);
			extensionList.AddRange(ExtensionMp4);
			extensionList.AddRange(ExtensionMts);
			extensionList.AddRange(ExtensionMjpeg);
			extensionList.AddRange(ExtensionWebp);
			extensionList.AddRange(ExtensionPsd);
			extensionList.AddRange(ExtensionRawSony);
			extensionList.AddRange(ExtensionRawAdobe);
			extensionList.AddRange(ExtensionRawNef);
			extensionList.AddRange(ExtensionRawRaf);
			extensionList.AddRange(ExtensionRawCr2);
			extensionList.AddRange(ExtensionRawCr3);
			extensionList.AddRange(ExtensionRawOrf);
			extensionList.AddRange(ExtensionRawRw2);
			extensionList.AddRange(ExtensionRawPef);
			extensionList.AddRange(ExtensionRawFff);
			extensionList.AddRange(ExtensionRawX3F);
			return extensionList;
		}
	}

	/// <summary>
	///     Gets the extension thumb supported list.
	///     ImageSharp => The IImageFormat interface, Jpeg, Png, Bmp, and Gif formats.
	///     Tiff based images are not supported by the thumbnail application
	/// </summary>
	/// <value>
	///     The extension thumb supported list.
	/// </value>
	public static List<string> ExtensionImageSharpThumbnailSupportedList
	{
		get
		{
			var extensionList = new List<string>();
			extensionList.AddRange(ExtensionJpg);
			extensionList.AddRange(ExtensionBmp);
			extensionList.AddRange(ExtensionGif);
			extensionList.AddRange(ExtensionPng);
			extensionList.AddRange(ExtensionWebp);
			return extensionList;
		}
	}

	public static List<string> ExtensionRawThumbnailSupportedList
	{
		get
		{
			var extensionList = new List<string>();
			extensionList.AddRange(ExtensionTiff);
			extensionList.AddRange(ExtensionRawSony);
			extensionList.AddRange(ExtensionRawAdobe);
			extensionList.AddRange(ExtensionRawNef);
			extensionList.AddRange(ExtensionRawRaf);
			extensionList.AddRange(ExtensionRawCr2);
			extensionList.AddRange(ExtensionRawCr3);
			extensionList.AddRange(ExtensionRawOrf);
			extensionList.AddRange(ExtensionRawRw2);
			extensionList.AddRange(ExtensionRawPef);
			extensionList.AddRange(ExtensionRawFff);
			extensionList.AddRange(ExtensionRawX3F);
			return extensionList;
		}
	}

	public static List<string> ExtensionVideoSupportedList
	{
		get
		{
			var extensionList = new List<string>();
			extensionList.AddRange(ExtensionMp4);
			extensionList.AddRange(ExtensionMts);
			extensionList.AddRange(ExtensionMjpeg);
			return extensionList;
		}
	}

	/// <summary>
	///     List of extension that are forced to use site car xmp files
	/// </summary>
	/// <value>
	///     The extension force XMP use list.
	/// </value>
	private static List<string> ExtensionForceXmpUseList
	{
		get
		{
			var extensionList = new List<string>();
			// add the sidecar files itself
			extensionList.AddRange(ExtensionXmp);
			extensionList.AddRange(ExtensionJsonSidecar);

			// Bitmap does not support internal xmp
			extensionList.AddRange(ExtensionBmp);
			// Gif does not support internal xmp
			extensionList.AddRange(ExtensionGif);
			// Used for raw files =>
			extensionList.AddRange(ExtensionTiff);
			// Raw formats need external XMP
			extensionList.AddRange(ExtensionRawSony);
			extensionList.AddRange(ExtensionRawAdobe);
			extensionList.AddRange(ExtensionRawNef);
			extensionList.AddRange(ExtensionRawCr2);
			extensionList.AddRange(ExtensionRawCr3);
			extensionList.AddRange(ExtensionRawOrf);
			extensionList.AddRange(ExtensionRawRw2);
			extensionList.AddRange(ExtensionRawPef);
			extensionList.AddRange(ExtensionRawRaf);
			extensionList.AddRange(ExtensionRawFff);
			extensionList.AddRange(ExtensionRawX3F);
			// reading does not allow xmp
			extensionList.AddRange(ExtensionMp4);
			extensionList.AddRange(ExtensionMts);
			extensionList.AddRange(ExtensionMjpeg);
			return extensionList;
		}
	}


	public static ImageFormat MapFileTypesToExtension(string filename)
	{
		if ( string.IsNullOrEmpty(filename) )
		{
			return ImageFormat.unknown;
		}

		var matchCollection = FileExtensionRegex().Matches(filename);
		if ( matchCollection.Count == 0 )
		{
			return ImageFormat.unknown;
		}

		var imageFormat = ImageFormat.unknown;
		foreach ( var matchValue in matchCollection.Select(p => p.Value) )
		{
			if ( matchValue.Length < 2 )
			{
				continue;
			}

			var ext = matchValue.Remove(0, 1).ToLowerInvariant();

			var extImageFormat = MapFileTypesToExtensionDictionary
				.FirstOrDefault(p => p.Value.Contains(ext)).Key;
			if ( extImageFormat != ImageFormat.unknown )
			{
				imageFormat = extImageFormat;
			}
		}

		return imageFormat;
	}

	/// <summary>
	///     is this filename with extension a file type that ExifTool can update
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <returns>true, if ExifTool can write to this</returns>
	public static bool IsExtensionExifToolSupported(string? filename)
	{
		if ( string.IsNullOrEmpty(filename) )
		{
			return false;
		}

		var extension = Path.GetExtension(filename);
		// dirs are = ""
		if ( string.IsNullOrEmpty(extension) )
		{
			return false;
		}

		var ext = extension.Remove(0, 1).ToLowerInvariant();
		return ExtensionExifToolSupportedList.Contains(ext); // true = if supported
	}

	/// <summary>
	///     Should we include this file in the database?
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <returns>true, </returns>
	public static bool IsExtensionSyncSupported(string filename)
	{
		return IsExtensionForce(filename.ToLowerInvariant(), ExtensionSyncSupportedList);
	}

	/// <summary>
	///     List of non-raw files supported by the raw preview extractor,
	/// which only supports jpg previews
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <returns>true, </returns>
	public static bool IsExtensionJpeg(string filename)
	{
		return IsExtensionForce(filename.ToLowerInvariant(), ExtensionJpg);
	}


	/// <summary>
	///     is this filename with extension a filetype that imageSharp can read/write
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <returns>true, if imageSharp can write to this</returns>
	public static bool IsExtensionImageSharpThumbnailSupported(string? filename)
	{
		return IsExtensionForce(filename?.ToLowerInvariant(),
			ExtensionImageSharpThumbnailSupportedList);
	}

	public static bool IsExtensionVideoSupported(string? fileName)
	{
		return IsExtensionForce(fileName?.ToLowerInvariant(), ExtensionVideoSupportedList);
	}

	public static bool IsExtensionRawThumbnailSupported(string? filename)
	{
		return IsExtensionForce(filename?.ToLowerInvariant(),
			ExtensionRawThumbnailSupportedList);
	}

	/// <summary>
	///     is this filename with extension a filetype that needs a .xmp file
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <returns>true, </returns>
	public static bool IsExtensionForceXmp(string? filename)
	{
		return IsExtensionForce(filename?.ToLowerInvariant(), ExtensionForceXmpUseList);
	}

	/// <summary>
	///     is this filename with extension a filetype that needs a .gpx file
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <returns>true, </returns>
	public static bool IsExtensionForceGpx(string? filename)
	{
		return IsExtensionForce(filename?.ToLowerInvariant(), ExtensionGpx);
	}

	/// <summary>
	///     Is the current file a sidecar file or not
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <returns>true, </returns>
	public static bool IsExtensionSidecar(string? filename)
	{
		var sidecars = ExtensionXmp.Concat(ExtensionJsonSidecar).ToList();
		return IsExtensionForce(filename?.ToLowerInvariant(), sidecars);
	}

	/// <summary>
	///     is this filename with extension a fileType that needs a item that is in the list
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <param name="checkThisList">the list of strings to match</param>
	/// <returns>true, </returns>
	// ReSharper disable once SuggestBaseTypeForParameter
	private static bool IsExtensionForce(string? filename, List<string> checkThisList)
	{
		if ( string.IsNullOrEmpty(filename) )
		{
			return false;
		}

		var matchCollection = FileExtensionRegex().Matches(filename);

		if ( matchCollection.Count == 0 )
		{
			return false;
		}

		foreach ( var matchValue in matchCollection.Select(p => p.Value) )
		{
			if ( matchValue.Length < 2 )
			{
				continue;
			}

			var ext = matchValue.Remove(0, 1).ToLowerInvariant();
			if ( checkThisList.Contains(ext) )
			{
				return true;
			}
		}

		// ReSharper disable once ConvertIfStatementToReturnStatement
		if ( filename.ToLowerInvariant().EndsWith(".meta.json") &&
		     checkThisList.Contains("meta.json") )
		{
			return true;
		}

		return false;
	}

	/// <summary>
	///     Extension must be three letters
	/// </summary>
	/// <param name="filename"></param>
	/// <returns></returns>
	public static string ReplaceExtensionWithXmp(string? filename)
	{
		if ( string.IsNullOrEmpty(filename) )
		{
			return string.Empty;
		}

		var matchCollection = FileExtensionRegex().Matches(filename);

		if ( matchCollection.Count == 0 )
		{
			return string.Empty;
		}

		foreach ( Match match in matchCollection )
		{
			if ( match.Value.Length < 2 )
			{
				continue;
			}

			// Extension must be three letters
			// removed: ExtensionForceXmpUseList.Contains(match.Value.Remove(0, 1).ToLowerInvariant()) && 
			if ( filename.Length >= match.Index + 4 )
			{
				var matchValue = filename.Substring(0, match.Index + 4).ToCharArray();
				matchValue[match.Index + 1] = 'x';
				matchValue[match.Index + 2] = 'm';
				matchValue[match.Index + 3] = 'p';
				return new string(matchValue);
			}
		}

		return string.Empty;
	}

	/// <summary>
	///     Check for file extensions
	///     without escaped values:
	///     \.([0-9a-z]+)(?=[?#])|(\.)(?:[\w]+)$
	/// </summary>
	/// <returns>Regex object</returns>
	[GeneratedRegex(
		@"\.([0-9a-z]+)(?=[?#])|(\.)(?:[\w]+)$",
		RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
		2000)]
	private static partial Regex FileExtensionRegex();

	[SuppressMessage("ReSharper", "MustUseReturnValue")]
	[SuppressMessage("Sonar", "S2674: stream.Read return value isn't used")]
	[SuppressMessage("ReSharper", "StreamReadReturnValueIgnored")]
	private byte[] ReadBuffer(Stream stream, int size)
	{
		var buffer = new byte[size];
		try
		{
			// Do not use ReadExactly here
			stream.Read(buffer, 0, buffer.Length);
			stream.Close();
			stream.Flush();
			stream.Dispose(); // also flush
		}
		catch ( UnauthorizedAccessException ex )
		{
			logger.LogError($"[ExtensionRoleHelper] ReadBuffer: {ex.Message}");
		}

		return buffer;
	}

	/// <summary>
	///     Get the format of the image by looking the first bytes
	///     Stream is Flushed / Disposed afterward
	/// </summary>
	/// <param name="stream">stream</param>
	/// <returns>ImageFormat enum</returns>
	public ImageFormat GetImageFormat(Stream stream)
	{
		if ( stream == Stream.Null )
		{
			return ImageFormat.notfound;
		}

		var format = GetImageFormat(ReadBuffer(stream, ImageFormatByteSize));
		return format;
	}

	/// <summary>
	///     Gets the image format. By first bytes
	/// </summary>
	/// <param name="bytes">The bytes of image</param>
	/// <returns>imageFormat enum</returns>
	public static ImageFormat GetImageFormat(byte[] bytes)
	{
		// see http://web.archive.org/web/20150524232918/http://www.mikekunz.com/image_file_header.html
		// on posix: 'od -t x1 -N 10 file.mp4'  
		var bmp = "BM"u8.ToArray(); // BMP
		var gif = "GIF"u8.ToArray(); // GIF
		var png = new byte[] { 137, 80, 78, 71 }; // PNG
		var pdf = "%PDF-"u8.ToArray(); // pdf

		if ( bmp.SequenceEqual(bytes.Take(bmp.Length)) )
		{
			return ImageFormat.bmp;
		}

		if ( gif.SequenceEqual(bytes.Take(gif.Length)) )
		{
			return ImageFormat.gif;
		}

		if ( png.SequenceEqual(bytes.Take(png.Length)) )
		{
			return ImageFormat.png;
		}

		var tiff = GetImageFormatRawTiff(bytes);
		if ( tiff != null )
		{
			return ( ImageFormat ) tiff;
		}

		if ( GetImageFormatJpeg(bytes) != null )
		{
			return ImageFormat.jpg;
		}

		if ( GetImageFormatXmp(bytes) != null )
		{
			return ImageFormat.xmp;
		}

		if ( GetImageFormatGpx(bytes) != null )
		{
			return ImageFormat.gpx;
		}

		var mpeg4 = GetImageFormatMpeg4(bytes);
		if ( mpeg4 != null )
		{
			return ( ImageFormat ) mpeg4;
		}

		if ( GetImageFormatMJpegFormat(bytes) != null )
		{
			return ImageFormat.mjpeg;
		}

		if ( GetImageFormatMtsFormat(bytes) != null )
		{
			return ImageFormat.mts;
		}

		if ( pdf.SequenceEqual(bytes.Take(pdf.Length)) )
		{
			return ImageFormat.pdf;
		}

		if ( GetImageFormatZip(bytes) != null )
		{
			return ImageFormat.zip;
		}

		if ( GetImageFormatMetaJson(bytes) != null )
		{
			return ImageFormat.meta_json;
		}

		if ( GetImageFormatMetaWebp(bytes) != null )
		{
			return ImageFormat.webp;
		}

		if ( GetImageFormatPsd(bytes) != null )
		{
			return ImageFormat.psd;
		}

		return ImageFormat.unknown;
	}

	private static ImageFormat? GetImageFormatPsd(byte[] bytes)
	{
		// HEX 38 42 50 53
		var psd = new byte[] { 56, 66, 80, 83 };
		if ( psd.SequenceEqual(bytes.Take(psd.Length)) )
		{
			return ImageFormat.psd;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatMetaWebp(byte[] bytes)
	{
		var webpFirstPart = new byte[] { 82, 73, 70, 70 };
		var webpSecondPart = new byte[] { 87, 69, 66, 80 };

		var isFirstPart = webpFirstPart.SequenceEqual(bytes.Take(webpFirstPart.Length));
		var isSecondPart = webpSecondPart.SequenceEqual(bytes.Skip(8).Take(webpSecondPart.Length));

		if ( isFirstPart && isSecondPart )
		{
			return ImageFormat.webp;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatMetaJson(byte[] bytes)
	{
		var metaJsonUnix = new byte[]
		{
			123, 10, 32, 32, 34, 36, 105, 100, 34, 58, 32, 34, 104, 116, 116, 112, 115, 58, 47,
			47, 100, 111, 99, 115, 46, 113, 100, 114, 97, 119, 46, 110, 108, 47, 115, 99, 104,
			101, 109, 97, 47, 109, 101, 116, 97, 45, 100, 97, 116, 97, 45, 99, 111, 110, 116,
			97, 105, 110, 101, 114, 46, 106, 115, 111, 110, 34, 44
		};
		// or : { \n "$id": "https://docs.qdraw.nl/schema/meta-data-container.json",

		var metaJsonWindows = new byte[]
		{
			// 13 is CR
			123, 13, 10, 32, 32, 34, 36, 105, 100, 34, 58, 32, 34, 104, 116, 116, 112, 115, 58, 47,
			47, 100, 111, 99, 115, 46, 113, 100, 114, 97, 119, 46, 110, 108, 47, 115, 99, 104, 101,
			109, 97, 47, 109, 101, 116, 97, 45, 100, 97, 116, 97, 45, 99, 111, 110, 116, 97, 105,
			110, 101, 114, 46, 106, 115, 111, 110, 34
		};

		if ( metaJsonUnix.SequenceEqual(bytes.Take(metaJsonUnix.Length)) )
		{
			return ImageFormat.meta_json;
		}

		if ( metaJsonWindows.SequenceEqual(bytes.Take(metaJsonWindows.Length)) )
		{
			return ImageFormat.meta_json;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatRawTiff(byte[] bytes)
	{
		if ( bytes.Length < 4 )
		{
			return null;
		}

		var value = ExtensionRaw.Detect(bytes);
		if ( value != ImageFormat.unknown )
		{
			return value;
		}

		var hasLittleEndianTiffHeader =
			bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00;
		var hasBigEndianTiffHeader =
			bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A;
		if ( !hasLittleEndianTiffHeader && !hasBigEndianTiffHeader )
		{
			return null;
		}

		// Default TIFF
		return ImageFormat.tiff;
	}


	private static ImageFormat? GetImageFormatMpeg4(byte[] bytes)
	{
		if ( bytes.Length < 8 )
		{
			return null;
		}

		// ISOBMFF: [size:4][ftyp:4][majorBrand:4]
		// Canon CR3 uses brand "crx "
		var fTypCr3 = "ftypcrx "u8.ToArray(); // ftypcrx 
		if ( bytes.Length >= 12 && fTypCr3.SequenceEqual(bytes.Skip(4).Take(fTypCr3.Length)) )
		{
			return ImageFormat.cr3;
		}

		var fTypMp4 = new byte[] { 102, 116, 121, 112 }; //  00  00  00  [skip this byte]
		// 66  74  79  70 QuickTime Container 3GG, 3GP, 3G2 	FLV

		if ( fTypMp4.SequenceEqual(bytes.Skip(4).Take(fTypMp4.Length)) )
		{
			return ImageFormat.mp4;
		}

		var fTypIsoM = new byte[] { 102, 116, 121, 112, 105, 115, 111, 109 };
		if ( fTypIsoM.SequenceEqual(bytes.Take(fTypIsoM.Length)) )
		{
			return ImageFormat.mp4;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatMJpegFormat(byte[] bytes)
	{
		var mjpegVideoFormat = new byte[] { 112, 110, 111, 116 };
		if ( mjpegVideoFormat.SequenceEqual(bytes.Skip(4).Take(mjpegVideoFormat.Length)) )
		{
			return ImageFormat.mjpeg;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatMtsFormat(byte[] bytes)
	{
		if ( bytes.Length < 44 )
		{
			return null;
		}

		int[] possibleOffsets = [0, 4];

		foreach ( var offset in possibleOffsets )
		{
			if ( bytes[offset] != 0x47 )
			{
				continue;
			}

			// Extract fields from TS header
			var b1 = bytes[offset + 1];
			var b2 = bytes[offset + 2];
			var b3 = bytes[offset + 3];

			var pid = ( ( b1 & 0x1F ) << 8 ) | b2;
			var adaptationFieldControl = ( b3 & 0x30 ) >> 4;

			var pidValid = pid != 0x1FFF;
			var adaptationValid = adaptationFieldControl != 0;

			if ( pidValid && adaptationValid )
			{
				return ImageFormat.mts;
			}
		}

		return null;
	}

	private static ImageFormat? GetImageFormatGpx(byte[] bytes)
	{
		var gpx = new byte[] { 60, 103, 112 }; // <gpx

		if ( gpx.SequenceEqual(bytes.Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(21).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(38).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(39).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(1).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(56).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(57).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(60).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(58).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(55).Take(gpx.Length)) )
		{
			return ImageFormat.gpx;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatZip(IEnumerable<byte> bytes)
	{
		var zip = new byte[] { 80, 75, 3, 4 };

		if ( zip.SequenceEqual(bytes.Take(zip.Length)) )
		{
			return ImageFormat.zip;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatXmp(byte[] bytes)
	{
		var xmp = "<x:xmpmeta"u8.ToArray(); // xmp
		var xmp2 = "<?xpacket"u8.ToArray(); // xmp

		if ( xmp.SequenceEqual(bytes.Take(xmp.Length)) )
		{
			return ImageFormat.xmp;
		}

		if ( xmp2.SequenceEqual(bytes.Take(xmp2.Length)) )
		{
			return ImageFormat.xmp;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatJpeg(byte[] bytes)
	{
		// https://en.wikipedia.org/wiki/List_of_file_signatures
		var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
		var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon
		var jpeg3 = new byte[] { 255, 216, 255, 219 }; // other jpeg
		var jpeg4 = new byte[] { 255, 216, 255, 237 }; // other ?
		var jpeg5 = new byte[] { 255, 216, 255, 238 }; // Hex: FF D8 FF EE 

		if ( jpeg.SequenceEqual(bytes.Take(jpeg.Length)) )
		{
			return ImageFormat.jpg;
		}

		if ( jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)) )
		{
			return ImageFormat.jpg;
		}

		if ( jpeg3.SequenceEqual(bytes.Take(jpeg3.Length)) )
		{
			return ImageFormat.jpg;
		}

		if ( jpeg4.SequenceEqual(bytes.Take(jpeg4.Length)) )
		{
			return ImageFormat.jpg;
		}

		if ( jpeg5.SequenceEqual(bytes.Take(jpeg5.Length)) )
		{
			return ImageFormat.jpg;
		}

		return null;
	}

	/// <summary>
	///     Convert Hex Value to byte array
	/// </summary>
	/// <param name="hex">hex value as string</param>
	/// <returns>byte value</returns>
	public static byte[] HexStringToByteArray(string hex)
	{
		return Enumerable.Range(0, hex.Length / 2)
			.Select(x => Convert.ToByte(
				hex.Substring(x * 2, 2), 16)
			).ToArray();
	}
}
