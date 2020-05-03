using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace starsky.foundation.platform.Helpers
{

	public static class ExtensionRolesHelper
	{
		/// <summary>
		/// List of .jpg,.jpeg extensions
		/// </summary>
		private static readonly List<string> ExtensionJpg = new List<string> {"jpg", "jpeg"};

		/// <summary>
		/// Tiff based, tiff, including raws
		/// tiff, arw:sony, dng:adobe, nef:nikon, raf:fuji, cr2:canon,  orf:olympus, rw2:panasonic, pef:pentax,
		/// Not supported due Image Processing Error x3f:sigma, crw:canon
		/// </summary>
		private static readonly List<string> ExtensionTiff = new List<string> {"tiff", "arw", "dng", "nef", 
			"raf", "cr2", "orf", "rw2", "pef"};

		/// <summary>
		/// Bitmaps
		/// </summary>
		private static readonly List<string> ExtensionBmp = new List<string> {"bmp"};
		
		/// <summary>
		/// Gif based images
		/// </summary>
		private static readonly List<string> ExtensionGif = new List<string> {"gif"};
		
		/// <summary>
		/// PNG
		/// </summary>
		private static readonly List<string> ExtensionPng = new List<string> {"png"};
		
		/// <summary>
		/// GPX, list of geo locations
		/// </summary>
		private static readonly List<string> ExtensionGpx = new List<string> {"gpx"};

		/// <summary>
		/// Mp4 Videos in h264 codex
		/// </summary>
		private static readonly List<string> ExtensionMp4 = new List<string> {"mp4", "mov"};

		private static readonly Dictionary<ImageFormat, List<string>> MapFileTypesToExtensionDictionary = 
			new Dictionary<ImageFormat, List<string>>
			{
				{
					ImageFormat.jpg, ExtensionJpg
				},
				{
					ImageFormat.tiff, ExtensionTiff
				},
				{
					ImageFormat.bmp, ExtensionBmp
				},
				{
					ImageFormat.gif, ExtensionGif
				},
				{
					ImageFormat.png, ExtensionPng
				},
				{
					ImageFormat.gpx, ExtensionGpx
				},
				{
					ImageFormat.mp4, ExtensionMp4
				},
			};

		
		public static ImageFormat MapFileTypesToExtension(string filename)
		{
			if ( string.IsNullOrEmpty(filename) ) return ImageFormat.unknown;

			// without escaped values:
			//		\.([0-9a-z]+)(?=[?#])|(\.)(?:[\w]+)$
			var matchCollection = new Regex("\\.([0-9a-z]+)(?=[?#])|(\\.)(?:[\\w]+)$").Matches(filename);
			if ( matchCollection.Count == 0 ) return ImageFormat.unknown;
			foreach ( Match match in matchCollection )
			{
				if ( match.Value.Length < 2 ) continue;
				var ext = match.Value.Remove(0, 1).ToLowerInvariant();

				return MapFileTypesToExtensionDictionary.FirstOrDefault(p => p.Value.Contains(ext)).Key;
			}
			return ImageFormat.unknown;
		}

		/// <summary>
		/// Supported by sync agent
		/// </summary>
		public static List<string> ExtensionSyncSupportedList
		{
			get
			{
				var extensionList = new List<string>();
				extensionList.AddRange(ExtensionJpg);
				extensionList.AddRange(ExtensionTiff);
				extensionList.AddRange(ExtensionBmp);
				extensionList.AddRange(ExtensionGif);
				extensionList.AddRange(ExtensionPng);
				extensionList.AddRange(ExtensionGpx);
				extensionList.AddRange(ExtensionMp4);
				return extensionList;
			}
		}

		/// <summary>
		/// List of extensions supported by exifTool
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
				return extensionList;
			}
		}

		/// <summary>
		/// is this filename with extension a file type that ExifTool can update
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <returns>true, if ExifTool can write to this</returns>
		public static bool IsExtensionExifToolSupported(string filename)
		{
			if ( string.IsNullOrEmpty(filename) ) return false;
			var extension = Path.GetExtension(filename);
			// dirs are = ""
			if ( string.IsNullOrEmpty(extension) ) return false;
			var ext = extension.Remove(0, 1).ToLowerInvariant();
			return ExtensionExifToolSupportedList.Contains(ext); // true = if supported
		}

		/// <summary>
		/// Gets the extension thumb supported list.
		/// ImageSharp => The IImageFormat interface, Jpeg, Png, Bmp, and Gif formats.
		/// Tiff based images are not supported by the thumbnail application 	
		/// </summary>
		/// <value>
		/// The extension thumb supported list.
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
				return extensionList;
			}
		}

		/// <summary>
		/// is this filename with extension a filetype that needs a .xmp file 
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <returns>true, </returns>
		public static bool IsExtensionSyncSupported(string filename)
		{
			return IsExtensionForce(filename.ToLowerInvariant(), ExtensionSyncSupportedList);
		}
		
		/// <summary>
		/// is this filename with extension a filetype that imagesharp can read/write 
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <returns>true, if imageSharp can write to this</returns>
		public static bool IsExtensionThumbnailSupported(string filename)
		{
			return IsExtensionForce(filename, ExtensionThumbSupportedList);
		}
		

		

		/// <summary>
		/// List of extension that are forced to use site car xmp files	
		/// </summary>
		/// <value>
		/// The extension force XMP use list.
		/// </value>
		private static List<string> ExtensionForceXmpUseList
		{
			get
			{
				var extensionList = new List<string>();
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

		/// <summary>
		/// is this filename with extension a filetype that needs a .xmp file 
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <returns>true, </returns>
		public static bool IsExtensionForceXmp(string filename)
		{
			return IsExtensionForce(filename, ExtensionForceXmpUseList);
		}

		/// <summary>
		/// is this filename with extension a filetype that needs a .gpx file 
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <returns>true, </returns>
		public static bool IsExtensionForceGpx(string filename)
		{
			return IsExtensionForce(filename, ExtensionGpx);
		}
		
		/// <summary>
		/// is this filename with extension a fileType that needs a item that is in the list 
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <param name="checkThisList">the list of strings to match</param>
		/// <returns>true, </returns>
		private static bool IsExtensionForce(string filename, List<string> checkThisList)
		{
			if ( string.IsNullOrEmpty(filename) ) return false;

			// without escaped values:
			//		\.([0-9a-z]+)(?=[?#])|(\.)(?:[\w]+)$
			var matchCollection = new Regex("\\.([0-9a-z]+)(?=[?#])|(\\.)(?:[\\w]+)$").Matches(filename);
			if ( matchCollection.Count == 0 ) return false;
			foreach ( Match match in matchCollection )
			{
				if ( match.Value.Length < 2 ) continue;
				var ext = match.Value.Remove(0, 1).ToLowerInvariant();
				if ( checkThisList.Contains(ext) ) return true;
			}
			return false;
		}
		
		/// <summary>
		/// Extension must be three letters
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static string ReplaceExtensionWithXmp(string filename)
		{
			// without escaped values:
			//		\.([0-9a-z]+)(?=[?#])|(\.)(?:[\w]+)$
			var matchCollection = new Regex("\\.([0-9a-z]+)(?=[?#])|(\\.)(?:[\\w]+)$").Matches(filename);
			if ( matchCollection.Count == 0 ) return string.Empty;
			foreach ( Match match in matchCollection )
			{
				if ( match.Value.Length < 2 ) continue;
				var ext = match.Value.Remove(0, 1).ToLowerInvariant();
				// Extension must be three letters
				if ( ExtensionForceXmpUseList.Contains(ext) && filename.Length >= match.Index + 4 )
				{
					var matchValue = filename.Substring(0, match.Index + 4).ToCharArray();
					
					matchValue[match.Index+1] = 'x';
					matchValue[match.Index+2] = 'm';
					matchValue[match.Index+3] = 'p';
					return new string(matchValue);
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// ImageFormat based on first bytes
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

			// Sidecar files
			xmp = 30,
            
			// documents
			gpx = 40,
			
			// video
			mp4 = 50
		}

		/// <summary>
		/// Get the format of the image by looking the first bytes
		/// </summary>
		/// <param name="stream">stream</param>
		/// <returns>ImageFormat enum</returns>
		public static ImageFormat GetImageFormat(Stream stream)
		{
			byte[] buffer = new byte[20];
			try
			{
				stream.Read(buffer, 0, buffer.Length);
				stream.Close();
				stream.Dispose();
			}
			catch ( UnauthorizedAccessException ex )
			{
				Console.WriteLine(ex.Message);
			}

			return GetImageFormat(buffer);
		}
		
		public static byte[] StringToByteArray(string hex)
		{
			return Enumerable.Range(0, hex.Length / 2)
				.Select(x => Convert.ToByte(hex.Substring(x * 2, 2), 16)).ToArray();
		}

		/// <summary>
		/// Gets the image format.
		/// </summary>
		/// <param name="bytes">The bytes of image</param>
		/// <returns>imageFormat enum</returns>
		public static ImageFormat GetImageFormat(byte[] bytes)
		{
			// see http://www.mikekunz.com/image_file_header.html  
			// on posix: 'od -t x1 -N 10 file.mp4'  
			var bmp = Encoding.ASCII.GetBytes("BM"); // BMP
			var gif = Encoding.ASCII.GetBytes("GIF"); // GIF
			var png = new byte[] {137, 80, 78, 71}; // PNG
			var tiff = new byte[] {73, 73, 42}; // TIFF
			var tiff2 = new byte[] {77, 77, 42}; // TIFF
			var dng = new byte[] {77, 77, 0}; // DNG? //0
			var olympusRaw  = new byte[] {73, 73, 82};
			var fujiFilmRaw = new byte[] {70, 85, 74};
			var panasonicRaw = new byte[] {73, 73, 85, 0};

			var jpeg = new byte[] {255, 216, 255, 224}; // jpeg
			var jpeg2 = new byte[] {255, 216, 255, 225}; // jpeg canon
			var jpeg3 = new byte[] {255, 216, 255, 219}; // other jpeg

			var xmp = Encoding.ASCII.GetBytes("<x:xmpmeta"); // xmp
			var gpx = new byte[] {60, 63, 120}; // gpx
			
			var fTypMp4 = new byte[] {102, 116, 121, 112}; //  00  00  00  [skip this byte]  66  74  79  70 QuickTime Container 3GG, 3GP, 3G2 	FLV

			// Zip:
			// 50 4B 03 04
			// 50 4B 05 06

			if ( bmp.SequenceEqual(bytes.Take(bmp.Length)) )
				return ImageFormat.bmp;

			if ( gif.SequenceEqual(bytes.Take(gif.Length)) )
				return ImageFormat.gif;

			if ( png.SequenceEqual(bytes.Take(png.Length)) )
				return ImageFormat.png;

			if ( tiff.SequenceEqual(bytes.Take(tiff.Length)) )
				return ImageFormat.tiff;

			if ( tiff2.SequenceEqual(bytes.Take(tiff2.Length)) )
				return ImageFormat.tiff;

			if ( dng.SequenceEqual(bytes.Take(dng.Length)) )
				return ImageFormat.tiff;
			
			if ( olympusRaw.SequenceEqual(bytes.Take(olympusRaw.Length)) )
				return ImageFormat.tiff;
			
			if ( fujiFilmRaw.SequenceEqual(bytes.Take(fujiFilmRaw.Length)) )
				return ImageFormat.tiff;
				
			if ( panasonicRaw.SequenceEqual(bytes.Take(panasonicRaw.Length)) )
				return ImageFormat.tiff;
			
			if ( jpeg.SequenceEqual(bytes.Take(jpeg.Length)) )
				return ImageFormat.jpg;

			if ( jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)) )
				return ImageFormat.jpg;
			
			if ( jpeg3.SequenceEqual(bytes.Take(jpeg3.Length)) )
				return ImageFormat.jpg;
			
			if ( xmp.SequenceEqual(bytes.Take(xmp.Length)) )
				return ImageFormat.xmp;

			if ( gpx.SequenceEqual(bytes.Take(gpx.Length)) )
				return ImageFormat.gpx;

			if ( fTypMp4.SequenceEqual(bytes.Skip(4).Take(fTypMp4.Length)) )
				return ImageFormat.mp4;
			
			return ImageFormat.unknown;
		}
	}
}
