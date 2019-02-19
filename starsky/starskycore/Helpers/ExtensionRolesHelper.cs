using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace starskycore.Helpers
{

	public static class ExtensionRolesHelper
	{
		private static readonly List<string> Extensionjpg = new List<string> {"jpg", "jpeg"};

		private static readonly List<string>
			Extensiontiff = new List<string> {"tiff", "arw", "dng"};

		private static readonly List<string> Extensionbmp = new List<string> {"bmp"};
		private static readonly List<string> Extensiongif = new List<string> {"gif"};
		private static readonly List<string> Extensionpng = new List<string> {"png"};
		private static readonly List<string> Extensiongpx = new List<string> {"gpx"};

		public static List<string> ExtensionSyncSupportedList
		{
			get
			{
				var extensionList = new List<string>();
				extensionList.AddRange(Extensionjpg);
				extensionList.AddRange(Extensiontiff);
				extensionList.AddRange(Extensionbmp);
				extensionList.AddRange(Extensiongif);
				extensionList.AddRange(Extensionpng);
				extensionList.AddRange(Extensiongpx);
				return extensionList;
			}
		}

		private static List<string> ExtensionExifToolSupportedList
		{
			get
			{
				var extensionList = new List<string>();
				extensionList.AddRange(Extensionjpg);
				extensionList.AddRange(Extensiontiff);
				extensionList.AddRange(Extensionbmp);
				extensionList.AddRange(Extensiongif);
				extensionList.AddRange(Extensionpng);
				return extensionList;
			}
		}

		/// <summary>
		/// is this filename with extension a filetype that exiftool can update
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <returns>true, if exiftool can write to this</returns>
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
		private static List<string> ExtensionThumbSupportedList
		{
			get
			{
				var extensionList = new List<string>();
				extensionList.AddRange(Extensionjpg);
				extensionList.AddRange(Extensionbmp);
				extensionList.AddRange(Extensiongif);
				extensionList.AddRange(Extensionpng);
				return extensionList;
			}
		}

		/// <summary>
		/// is this filename with extension a filetype that imagesharp can read/write 
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <returns>true, if imagesharp can write to this</returns>
		public static bool IsExtensionThumbnailSupported(string filename)
		{
			var ext = Path.GetExtension(filename).Remove(0, 1).ToLowerInvariant();
			return ExtensionThumbSupportedList.Contains(ext); // true = if supported
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
				extensionList.AddRange(Extensionbmp);
				// Gif does not support internal xmp
				extensionList.AddRange(Extensiongif);
				// Used for raw files >
				extensionList.AddRange(Extensiontiff);
				return extensionList;
			}
		}

		/// <summary>
		/// used for raw, bmp filetypes that has no support for in file exif
		/// </summary>
		/// <param name="filename">the name of the file with extenstion</param>
		/// <returns>true, if Sidecar is required</returns>
		public static bool IsXmpSidecarRequired(string fullFilePath)
		{
			if ( string.IsNullOrEmpty(fullFilePath) ) return false;
			// Use an XMP File -> as those files don't support those tags
			if ( ExtensionForceXmpUseList.Contains(Path.GetExtension(fullFilePath)
				.Replace(".", string.Empty).ToLower()) )
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get the sitecar file of the raw image
		/// </summary>
		/// <param name="fullFilePath">the full path on the system</param>
		/// <param name="exifToolXmpPrefix">prefix</param>
		/// <returns>full file path of sitecar file</returns>
		public static string GetXmpSidecarFileWhenRequired(
			string fullFilePath,
			string exifToolXmpPrefix = "")
		{
			if ( exifToolXmpPrefix == null )
				throw new ArgumentNullException(nameof(exifToolXmpPrefix));
			// Use an XMP File -> as those files don't support those tags
			if ( IsXmpSidecarRequired(fullFilePath) )
			{
				return GetXmpSidecarFile(fullFilePath, exifToolXmpPrefix);
			}

			return fullFilePath;
		}

		public static string GetXmpSidecarFile(
			string fullFilePath,
			string exifToolXmpPrefix = "")
		{
			// Overwrite to use xmp files
			return Path.Combine(Path.GetDirectoryName(fullFilePath),
				exifToolXmpPrefix
				+ Path.GetFileNameWithoutExtension(fullFilePath) + ".xmp");
		}
		
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

			// Sitecar files
			xmp = 30,
            
			// documents
			gpx = 40
		}

		/// <summary>
		/// Get the format of the image by looking the first bytes
		/// </summary>
		/// <param name="filePath">the full path on the system</param>
		/// <returns>ImageFormat enum</returns>
		public static ImageFormat GetImageFormat(string filePath)
		{
			if ( !File.Exists(filePath) ) return ImageFormat.notfound;

			byte[] buffer = new byte[512];
			try
			{
				using ( FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read) )
				{
					fs.Read(buffer, 0, buffer.Length);
					fs.Close();
				}
			}
			catch ( UnauthorizedAccessException ex )
			{
				Console.WriteLine(ex.Message);
			}

			return GetImageFormat(buffer);
		}

		/// <summary>
		/// Gets the image format.
		/// </summary>
		/// <param name="bytes">The bytes of image</param>
		/// <returns></returns>
		public static ImageFormat GetImageFormat(byte[] bytes)
		{
			// see http://www.mikekunz.com/image_file_header.html  
			var bmp = Encoding.ASCII.GetBytes("BM"); // BMP
			var gif = Encoding.ASCII.GetBytes("GIF"); // GIF
			var png = new byte[] {137, 80, 78, 71}; // PNG
			var tiff = new byte[] {73, 73, 42}; // TIFF
			var tiff2 = new byte[] {77, 77, 42}; // TIFF
			var tiff3 = new byte[] {77, 77, 0}; // DNG? //0
			var jpeg = new byte[] {255, 216, 255, 224}; // jpeg
			var jpeg2 = new byte[] {255, 216, 255, 225}; // jpeg canon
			var xmp = Encoding.ASCII.GetBytes("<x:xmpmeta"); // xmp
			var gpx = new byte[] {60, 63, 120}; // gpx

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

			if ( tiff3.SequenceEqual(bytes.Take(tiff3.Length)) )
				return ImageFormat.tiff;

			if ( jpeg.SequenceEqual(bytes.Take(jpeg.Length)) )
				return ImageFormat.jpg;

			if ( jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)) )
				return ImageFormat.jpg;

			if ( xmp.SequenceEqual(bytes.Take(xmp.Length)) )
				return ImageFormat.xmp;

			if ( gpx.SequenceEqual(bytes.Take(gpx.Length)) )
				return ImageFormat.gpx;

			return ImageFormat.unknown;
		}
	}
}
