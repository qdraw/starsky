using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

public class ShellThumbnailExtractionWindows
{
	public static void GenerateThumbnail(string inputPath, string outputBmpPath, int width, int height)
	{
		var iid = typeof(IShellItemImageFactory).GUID;
		int hr = SHCreateItemFromParsingName(inputPath, IntPtr.Zero, ref iid, out var factoryObj);
		if ( hr != 0 || factoryObj is not IShellItemImageFactory factory )
			throw new COMException("Failed to get IShellItemImageFactory", hr);

		var size = new SIZE { cx = width, cy = height };
		factory.GetImage(size, SIIGBF.SIIGBF_RESIZETOFIT, out var hBitmap);

		if ( hBitmap == IntPtr.Zero )
			throw new Exception("Failed to get HBITMAP.");

		SaveHBitmapToBmp(hBitmap, outputBmpPath);
		DeleteObject(hBitmap); // prevent memory leak
	}

	private static void SaveHBitmapToBmp(IntPtr hBitmap, string outputPath)
	{
		BITMAP bmp;
		if ( GetObject(hBitmap, Marshal.SizeOf<BITMAP>(), out bmp) == 0 )
			throw new Exception("GetObject failed.");

		int headerSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER)) + Marshal.SizeOf(typeof(BITMAPINFOHEADER));
		int pixelDataSize = bmp.bmWidthBytes * bmp.bmHeight;
		int totalSize = headerSize + pixelDataSize;

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
			biBitCount = ( ushort ) bmp.bmBitsPixel,
			biCompression = 0, // BI_RGB
			biSizeImage = ( uint ) pixelDataSize
		};

		// Write headers
		WriteStruct(fs, fileHeader);
		WriteStruct(fs, infoHeader);

		// Copy pixel data
		byte[] pixelBytes = new byte[pixelDataSize];
		Marshal.Copy(bmp.bmBits, pixelBytes, 0, pixelDataSize);
		fs.Write(pixelBytes, 0, pixelBytes.Length);
	}

	private static void WriteStruct<T>(Stream s, T strct)
	{
		byte[] bytes = new byte[Marshal.SizeOf<T>()];
		IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
		Marshal.StructureToPtr(strct, ptr, false);
		Marshal.Copy(ptr, bytes, 0, bytes.Length);
		Marshal.FreeHGlobal(ptr);
		s.Write(bytes, 0, bytes.Length);
	}

	// Native interop
	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll", SetLastError = true)]
	private static extern int GetObject(IntPtr hgdiobj, int cbBuffer, out BITMAP lpvObject);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHCreateItemFromParsingName(
		string path, IntPtr pbc, ref Guid riid, out object ppv);

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
	private interface IShellItemImageFactory
	{
		void GetImage(SIZE size, SIIGBF flags, out IntPtr phbm);
	}

	private enum SIIGBF
	{
		SIIGBF_RESIZETOFIT = 0x00,
		SIIGBF_BIGGERSIZEOK = 0x01,
		SIIGBF_MEMORYONLY = 0x02,
		SIIGBF_ICONONLY = 0x04,
		SIIGBF_THUMBNAILONLY = 0x08,
		SIIGBF_INCACHEONLY = 0x10
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct SIZE
	{
		public int cx;
		public int cy;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct BITMAP
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
	private struct BITMAPFILEHEADER
	{
		public ushort bfType;
		public uint bfSize;
		public ushort bfReserved1;
		public ushort bfReserved2;
		public uint bfOffBits;
	}

	[StructLayout(LayoutKind.Sequential)]
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
