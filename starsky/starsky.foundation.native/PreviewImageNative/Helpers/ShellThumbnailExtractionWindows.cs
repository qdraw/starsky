using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

[SuppressMessage("Interoperability",
	"SYSLIB1054:Use \'LibraryImportAttribute\' instead of \'DllImportAttribute\' " +
	"to generate P/Invoke marshalling code at compile time")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ShellThumbnailExtractionWindows(IWebLogger logger)
{
	public static bool IsSupported(int width = 512, int height = 512)
	{
		return width <= 512 && height <= 512 &&
		       RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
		       RuntimeInformation.OSArchitecture == Architecture.X64;
	}

	public bool GenerateThumbnail(string inputPath, string outputBmpPath, int width,
		int height)
	{
		return IsSupported(width, height) &&
		       GenerateThumbnailInternal(inputPath, outputBmpPath, width, height);
	}

	internal bool GenerateThumbnailInternal(string inputPath, string outputBmpPath,
		int width,
		int height)
	{
		if ( height <= 0 )
		{
			throw new ArgumentException("Height must be greater than zero.", nameof(height));
		}

		var size = new SIZE { cx = width, cy = height };

		try
		{
			var factory = ShCreateItemFromParsingName(inputPath);
			factory.GetImage(size,
				SIIGBF.SIIGBF_RESIZETOFIT | SIIGBF.SIIGBF_THUMBNAILONLY, out var hBitmap);
			SaveHBitmapToBmp(hBitmap, outputBmpPath);
			DeleteObject(hBitmap); // prevent memory leak
		}
		catch ( Exception )
		{
			logger.LogInformation(
				"[ShellThumbnailExtractionWindows] Error: Failed to create URL for {filePath}",
				inputPath);
			return false;
		}

		return true;
	}

	internal static void SaveHBitmapToBmp(IntPtr hBitmap, string outputPath)
	{
		if ( GetObject(hBitmap, Marshal.SizeOf<BITMAP>(), out var bmp) == 0 )
		{
			throw new InvalidOperationException("GetObject failed.");
		}

		SaveHBitmapToBmp(bmp, outputPath);
	}

	internal static void SaveHBitmapToBmp(BITMAP bmp, string outputPath)
	{
		var headerSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER)) +
		                 Marshal.SizeOf(typeof(BITMAPINFOHEADER));
		var pixelDataSize = bmp.bmWidthBytes * bmp.bmHeight;
		var totalSize = headerSize + pixelDataSize;

		using var fs = new FileStream(outputPath, FileMode.Create);

		var fileHeader = new BITMAPFILEHEADER
		{
			bfType = 0x4D42, // 'BM'
			bfSize = ( uint ) totalSize,
			bfOffBits = ( uint ) headerSize
		};

		var infoHeader = new BITMAPINFOHEADER
		{
			biSize = ( uint ) Marshal.SizeOf(typeof(BITMAPINFOHEADER)),
			biWidth = bmp.bmWidth,
			biHeight = bmp.bmHeight,
			biPlanes = 1,
			biBitCount = bmp.bmBitsPixel,
			biCompression = 0, // BI_RGB
			biSizeImage = ( uint ) pixelDataSize
		};

		// Write headers
		WriteStruct(fs, fileHeader);
		WriteStruct(fs, infoHeader);

		// Copy pixel data
		var pixelBytes = new byte[pixelDataSize];
		Marshal.Copy(bmp.bmBits, pixelBytes, 0, pixelDataSize);

		var flippedImage = FlipImage(bmp, pixelBytes, pixelDataSize);

		fs.Write(flippedImage, 0, pixelBytes.Length);
	}

	internal static byte[] FlipImage(BITMAP bmp, byte[] pixelBytes, int pixelDataSize)
	{
		var flippedBytes = new byte[pixelDataSize];

		var rowSize = bmp.bmWidthBytes;
		for ( var y = 0; y < bmp.bmHeight; y++ )
		{
			Array.Copy(pixelBytes, y * rowSize, flippedBytes,
				( bmp.bmHeight - 1 - y ) * rowSize, rowSize);
		}

		return flippedBytes;
	}

	internal static void WriteStruct<T>(Stream s, T? strct)
	{
		if ( EqualityComparer<T>.Default.Equals(strct, default) )
		{
			throw new ArgumentNullException(nameof(strct));
		}

		var bytes = new byte[Marshal.SizeOf<T>()];
		var ptr = Marshal.AllocHGlobal(bytes.Length);
		Marshal.StructureToPtr(strct!, ptr, false);
		Marshal.Copy(ptr, bytes, 0, bytes.Length);
		Marshal.FreeHGlobal(ptr);
		s.Write(bytes, 0, bytes.Length);
	}

	// Native interop
	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll", SetLastError = true)]
	private static extern int GetObject(IntPtr hgdiobj, int cbBuffer, out BITMAP lpvObject);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
	private static extern void SHCreateItemFromParsingName(
		[MarshalAs(UnmanagedType.LPWStr)] string pszPath,
		IntPtr pbc,
		[MarshalAs(UnmanagedType.LPStruct)] Guid riid,
		[MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory ppv);

	private static IShellItemImageFactory ShCreateItemFromParsingName(string inputPath)
	{
		var factoryGuid = typeof(IShellItemImageFactory).GUID;
		SHCreateItemFromParsingName(inputPath, IntPtr.Zero, factoryGuid, out var factory);
		return factory;
	}

	[SuppressMessage("Interoperability",
		"SYSLIB1096:Mark the type 'IShellItemImageFactory' with " +
		"'GeneratedComInterfaceAttribute' instead of 'ComImportAttribute' " +
		"to generate COM marshalling code at compile time")]
	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
	private interface IShellItemImageFactory
	{
		void GetImage(SIZE size, SIIGBF flags, out IntPtr phbm);
	}

	[Flags]
	[SuppressMessage("Usage", "S2346: " +
	                          "Rename 'SIIGBF_RESIZETOFIT' to 'None'.")]
	[SuppressMessage("Usage", "S2342: " +
	                          "Enumeration types should comply with a naming convention")]
	internal enum SIIGBF
	{
		SIIGBF_RESIZETOFIT = 0x00,
		SIIGBF_BIGGERSIZEOK = 0x01,
		SIIGBF_MEMORYONLY = 0x02,
		SIIGBF_ICONONLY = 0x04,
		SIIGBF_THUMBNAILONLY = 0x08,
		SIIGBF_INCACHEONLY = 0x10
	}

	[StructLayout(LayoutKind.Sequential)]
	[SuppressMessage("Usage", "S101: Rename class 'SIZE' " +
	                          "to match pascal case naming rules, consider using 'Size'.")]
	private struct SIZE
	{
		public int cx;
		public int cy;
	}

	[StructLayout(LayoutKind.Sequential)]
	[SuppressMessage("Usage", "S101: Rename class 'BITMAP' " +
	                          "to match pascal case naming rules, consider using 'Bitmap'.")]
	internal struct BITMAP
	{
		public int bmType;
		public int bmWidth;
		public int bmHeight;
		public int bmWidthBytes;
		public ushort bmPlanes;
		public ushort bmBitsPixel;
		public IntPtr bmBits;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[SuppressMessage("Usage", "S101: Rename class 'BITMAPFILEHEADER' " +
	                          "to match pascal case naming rules, consider using 'BitmapFileHeader'.")]
	private struct BITMAPFILEHEADER
	{
		public ushort bfType;
		public uint bfSize;
		public ushort bfReserved1;
		public ushort bfReserved2;
		public uint bfOffBits;
	}

	[StructLayout(LayoutKind.Sequential)]
	[SuppressMessage("Usage", "S101: Rename class 'BITMAPINFOHEADER' " +
	                          "to match pascal case naming rules, consider using 'BitmapInfoHeader'.")]
	private struct BITMAPINFOHEADER
	{
		public uint biSize;
		public int biWidth;
		public int biHeight;
		public ushort biPlanes;
		public ushort biBitCount;
		public uint biCompression;
		public uint biSizeImage;
		public int biXPelsPerMeter;
		public int biYPelsPerMeter;
		public uint biClrUsed;
		public uint biClrImportant;
	}
}
