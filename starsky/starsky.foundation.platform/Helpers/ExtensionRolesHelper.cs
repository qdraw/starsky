using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers;

public static partial class ExtensionRolesHelper
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

		// Sidecar files
		xmp = 30,

		/// <summary>
		///     Extension: .meta.json
		/// </summary>
		meta_json = 31,

		// documents
		gpx = 40,
		pdf = 41,

		// video
		mp4 = 50,

		// archives
		zip = 60,

		// folder
		directory = 1000
	}

	/// <summary>
	///     Xmp sidecar file
	/// </summary>
	private static readonly List<string> ExtensionXmp = new() { "xmp" };

	/// <summary>
	///     Meta.json sidecar files
	/// </summary>
	private static readonly List<string>
		ExtensionJsonSidecar = new() { "meta.json" };


	/// <summary>
	///     List of .jpg,.jpeg extensions
	/// </summary>
	private static readonly List<string> ExtensionJpg = new() { "jpg", "jpeg" };

	/// <summary>
	///     Tiff based, tiff, including raws
	///     tiff, arw:sony, dng:adobe, nef:nikon, raf:fuji, cr2:canon,  orf:olympus, rw2:panasonic,
	///     pef:pentax,
	///     Not supported due Image Processing Error x3f:sigma, crw:canon
	/// </summary>
	private static readonly List<string> ExtensionTiff = new()
	{
		"tiff",
		"arw",
		"dng",
		"nef",
		"raf",
		"cr2",
		"orf",
		"rw2",
		"pef"
	};

	/// <summary>
	///     Bitmaps
	/// </summary>
	private static readonly List<string> ExtensionBmp = new() { "bmp" };

	/// <summary>
	///     Gif based images
	/// </summary>
	private static readonly List<string> ExtensionGif = new() { "gif" };

	/// <summary>
	///     PNG
	/// </summary>
	private static readonly List<string> ExtensionPng = new() { "png" };

	/// <summary>
	///     GPX, list of geolocations
	/// </summary>
	private static readonly List<string> ExtensionGpx = new() { "gpx" };

	/// <summary>
	///     Mp4 Videos in h264 codex
	/// </summary>
	private static readonly List<string> ExtensionMp4 = new() { "mp4", "mov" };

	/// <summary>
	///     WebP imageFormat
	/// </summary>
	private static readonly List<string> ExtensionWebp = new() { "webp" };

	/// <summary>
	///     Psd imageFormat
	/// </summary>
	private static readonly List<string> ExtensionPsd = new() { "psd" };

	private static readonly Dictionary<ImageFormat, List<string>>
		MapFileTypesToExtensionDictionary =
			new()
			{
				{ ImageFormat.jpg, ExtensionJpg },
				{ ImageFormat.tiff, ExtensionTiff },
				{ ImageFormat.bmp, ExtensionBmp },
				{ ImageFormat.gif, ExtensionGif },
				{ ImageFormat.png, ExtensionPng },
				{ ImageFormat.gpx, ExtensionGpx },
				{ ImageFormat.mp4, ExtensionMp4 },
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
			extensionList.AddRange(ExtensionBmp);
			extensionList.AddRange(ExtensionGif);
			extensionList.AddRange(ExtensionPng);
			extensionList.AddRange(ExtensionGpx);
			extensionList.AddRange(ExtensionMp4);
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
			extensionList.AddRange(ExtensionWebp);
			extensionList.AddRange(ExtensionPsd);
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
	public static List<string> ExtensionThumbSupportedList
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
			// reading does not allow xmp
			extensionList.AddRange(ExtensionMp4);
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
	///     is this filename with extension a filetype that imageSharp can read/write
	/// </summary>
	/// <param name="filename">the name of the file with extenstion</param>
	/// <returns>true, if imageSharp can write to this</returns>
	public static bool IsExtensionThumbnailSupported(string? filename)
	{
		return IsExtensionForce(filename?.ToLowerInvariant(), ExtensionThumbSupportedList);
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
	private static byte[] ReadBuffer(Stream stream, int size)
	{
		var buffer = new byte[size];
		try
		{
			stream.Read(buffer, 0, buffer.Length);
			stream.Close();
			stream.Flush();
			stream.Dispose(); // also flush
		}
		catch ( UnauthorizedAccessException ex )
		{
			Console.WriteLine(ex.Message);
		}

		return buffer;
	}


	/// <summary>
	///     Get the format of the image by looking the first bytes
	///     Stream is Flushed / Disposed afterward
	/// </summary>
	/// <param name="stream">stream</param>
	/// <returns>ImageFormat enum</returns>
	public static ImageFormat GetImageFormat(Stream stream)
	{
		if ( stream == Stream.Null )
		{
			return ImageFormat.notfound;
		}

		var format = GetImageFormat(ReadBuffer(stream, 68));
		return format;
	}

	/// <summary>
	///     Gets the image format.
	/// </summary>
	/// <param name="bytes">The bytes of image</param>
	/// <returns>imageFormat enum</returns>
	public static ImageFormat GetImageFormat(byte[] bytes)
	{
		// see http://web.archive.org/web/20150524232918/http://www.mikekunz.com/image_file_header.html
		// on posix: 'od -t x1 -N 10 file.mp4'  
		var bmp = Encoding.ASCII.GetBytes("BM"); // BMP
		var gif = Encoding.ASCII.GetBytes("GIF"); // GIF
		var png = new byte[] { 137, 80, 78, 71 }; // PNG
		var pdf = new byte[] { 37, 80, 68, 70, 45 }; // pdf

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

		if ( GetImageFormatTiff(bytes) != null )
		{
			return ImageFormat.tiff;
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

		if ( GetImageFormatMpeg4(bytes) != null )
		{
			return ImageFormat.mp4;
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

	private static ImageFormat? GetImageFormatTiff(byte[] bytes)
	{
		var tiff = new byte[] { 73, 73, 42 }; // TIFF
		var tiff2 = new byte[] { 77, 77, 42 }; // TIFF
		var dng = new byte[] { 77, 77, 0 }; // DNG? //0
		var olympusRaw = new byte[] { 73, 73, 82 };
		var fujiFilmRaw = new byte[] { 70, 85, 74 };
		var panasonicRaw = new byte[] { 73, 73, 85, 0 };

		if ( tiff.SequenceEqual(bytes.Take(tiff.Length)) )
		{
			return ImageFormat.tiff;
		}

		if ( tiff2.SequenceEqual(bytes.Take(tiff2.Length)) )
		{
			return ImageFormat.tiff;
		}

		if ( dng.SequenceEqual(bytes.Take(dng.Length)) )
		{
			return ImageFormat.tiff;
		}

		if ( olympusRaw.SequenceEqual(bytes.Take(olympusRaw.Length)) )
		{
			return ImageFormat.tiff;
		}

		if ( fujiFilmRaw.SequenceEqual(bytes.Take(fujiFilmRaw.Length)) )
		{
			return ImageFormat.tiff;
		}

		if ( panasonicRaw.SequenceEqual(bytes.Take(panasonicRaw.Length)) )
		{
			return ImageFormat.tiff;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatMpeg4(byte[] bytes)
	{
		var fTypMp4 = new byte[] { 102, 116, 121, 112 }; //  00  00  00  [skip this byte]
		// 66  74  79  70 QuickTime Container 3GG, 3GP, 3G2 	FLV

		if ( fTypMp4.SequenceEqual(bytes.Skip(4).Take(fTypMp4.Length)) )
		{
			return ImageFormat.mp4;
		}

		var fTypIsoM = new byte[] { 102, 116, 121, 112, 105, 115, 111, 109 };
		if ( fTypIsoM.SequenceEqual(bytes.Take(fTypIsoM.Length)) )
		{
			return ImageFormat.xmp;
		}

		return null;
	}

	private static ImageFormat? GetImageFormatGpx(byte[] bytes)
	{
		var gpx = new byte[] { 60, 103, 112 }; // <gpx

		if ( gpx.SequenceEqual(bytes.Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(21).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(39).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(1).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(56).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(57).Take(gpx.Length)) ||
		     gpx.SequenceEqual(bytes.Skip(60).Take(gpx.Length)) ||
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
		var xmp = Encoding.ASCII.GetBytes("<x:xmpmeta"); // xmp
		var xmp2 = Encoding.ASCII.GetBytes("<?xpacket"); // xmp

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
