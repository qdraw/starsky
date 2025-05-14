using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

internal class ShellThumbnailExtractionWindows
{
	private static bool GenerateThumbnail(string filePath, string outputPath, int width,
		int height)
	{
		var shellItemGuid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"); // IShellItem
		var hr = SHCreateItemFromParsingName(filePath, IntPtr.Zero, ref shellItemGuid,
			out var shellItem);
		if ( hr != 0 )
		{
			return false;
		}

		var factory = ( IShellItemImageFactory ) shellItem;

		var size = new SIZE { cx = width, cy = height };
		var flags = SIIGBF.RESIZETOFIT;

		IntPtr hBitmap;
		hr = factory.GetImage(size, flags, out hBitmap);
		if ( hr != 0 )
		{
			return false;
		}

		var wicFactory = ( IWICImagingFactory ) new WICImagingFactory();
		IWICBitmap wicBitmap;
		wicFactory.CreateBitmapFromHBITMAP(hBitmap, IntPtr.Zero,
			WICBitmapAlphaChannelOption.WICBitmapUseAlpha, out wicBitmap);

		IStream stream;
		CreateStreamOnHGlobal(IntPtr.Zero, true, out stream);

		var jpegFormat =
			new Guid("19e4a5aa-5662-4fc5-a0c0-1758028e1057"); // GUID_ContainerFormatJpeg
		IWICBitmapEncoder encoder;
		wicFactory.CreateEncoder(ref jpegFormat, IntPtr.Zero, out encoder);
		encoder.Initialize(stream, WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache);

		encoder.CreateNewFrame(out var frameEncode, out _);
		frameEncode.Initialize(null);
		frameEncode.SetSize(( uint ) width, ( uint ) height);

		var pixelFormat =
			new Guid("6fddc324-4e03-4bfe-b185-3d77768dc901"); // GUID_WICPixelFormat32bppPBGRA
		frameEncode.SetPixelFormat(ref pixelFormat);
		frameEncode.WriteSource(wicBitmap, IntPtr.Zero);
		frameEncode.Commit();
		encoder.Commit();

		// Write to file
		STATSTG stats;
		stream.Stat(out stats, 0);
		var buffer = new byte[stats.cbSize];
		stream.Seek(0, 0, IntPtr.Zero);
		stream.Read(buffer, buffer.Length, IntPtr.Zero);
		File.WriteAllBytes(outputPath, buffer);

		DeleteObject(hBitmap);
		return true;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
	private static extern int SHCreateItemFromParsingName(
		[MarshalAs(UnmanagedType.LPWStr)] string pszPath,
		IntPtr pbc,
		ref Guid riid,
		[MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(IntPtr hObject);

	[DllImport("ole32.dll")]
	private static extern int CreateStreamOnHGlobal(IntPtr hGlobal, bool fDeleteOnRelease,
		out IStream ppstm);

	[ComImport]
	[Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IShellItem
	{
	}

	[ComImport]
	[Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IShellItemImageFactory
	{
		int GetImage(SIZE size, SIIGBF flags, out IntPtr phbm);
	}

	private enum SIIGBF
	{
		RESIZETOFIT = 0x00,
		BIGGERSIZEOK = 0x01,
		MEMORYONLY = 0x02,
		ICONONLY = 0x04,
		THUMBNAILONLY = 0x08,
		INCACHEONLY = 0x10
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct SIZE
	{
		public int cx;
		public int cy;
	}

	[ComImport]
	[Guid("EC5EC8A9-C395-4314-9C77-54D7A935FF70")]
	private class WICImagingFactory
	{
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("EC5EC8A9-C395-4314-9C77-54D7A935FF70")]
	private interface IWICImagingFactory
	{
		void _VtblGap1_18(); // skip unused methods

		void CreateBitmapFromHBITMAP(IntPtr hBitmap, IntPtr hPalette,
			WICBitmapAlphaChannelOption options, out IWICBitmap ppIBitmap);

		void _VtblGap2_2(); // skip more

		void CreateEncoder(ref Guid guidContainerFormat, IntPtr pguidVendor,
			out IWICBitmapEncoder ppIEncoder);
	}

	private enum WICBitmapAlphaChannelOption
	{
		WICBitmapUseAlpha = 0
	}

	private enum WICBitmapEncoderCacheOption
	{
		WICBitmapEncoderNoCache = 0
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("00000105-A8F2-4877-BA0A-FD2B6645FB94")]
	private interface IWICBitmapEncoder
	{
		void Initialize(IStream pIStream, WICBitmapEncoderCacheOption cacheOption);

		void CreateNewFrame(out IWICBitmapFrameEncode ppIFrameEncode,
			out IPropertyBag2 ppIEncoderOptions);

		void Commit();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("00000121-a8f2-4877-ba0a-fd2b6645fb94")]
	private interface IWICBitmapFrameEncode
	{
		void Initialize(IPropertyBag2 pIEncoderOptions);
		void SetSize(uint uiWidth, uint uiHeight);
		void SetPixelFormat(ref Guid pPixelFormat);
		void WriteSource(IWICBitmap pIBitmapSource, IntPtr prc);
		void Commit();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("0000010c-0000-0000-C000-000000000046")]
	private interface IPropertyBag2
	{
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("00000120-a8f2-4877-ba0a-fd2b6645fb94")]
	private interface IWICBitmap
	{
	}
}
