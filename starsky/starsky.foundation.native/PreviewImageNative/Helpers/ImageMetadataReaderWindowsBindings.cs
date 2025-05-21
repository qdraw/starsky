using System.Runtime.InteropServices;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

public static class ImageMetadataReaderWindowsBindings
{
	[Flags]
	public enum CLSCTX : uint
	{
		INPROC_SERVER = 0x1,
		INPROC_HANDLER = 0x2,
		LOCAL_SERVER = 0x4,
		INPROC_SERVER16 = 0x8,
		REMOTE_SERVER = 0x10,
		INPROC_HANDLER16 = 0x20,
		RESERVED1 = 0x40,
		RESERVED2 = 0x80,
		RESERVED3 = 0x100,
		RESERVED4 = 0x200,
		NO_CODE_DOWNLOAD = 0x400,
		RESERVED5 = 0x800,
		NO_CUSTOM_MARSHAL = 0x1000,
		ENABLE_CODE_DOWNLOAD = 0x2000,
		NO_FAILURE_LOG = 0x4000,
		DISABLE_AAA = 0x8000,
		ENABLE_AAA = 0x10000,
		FROM_DEFAULT_CONTEXT = 0x20000,
		ACTIVATE_X86_SERVER = 0x40000,
		ACTIVATE_32_BIT_SERVER = 0x40000,
		ACTIVATE_64_BIT_SERVER = 0x80000,
		ENABLE_CLOAKING = 0x100000,
		PS_DLL = 0x80000000
	}

	[Flags]
	public enum CoInit : uint
	{
		MultiThreaded = 0x0,
		ApartmentThreaded = 0x2,
		DisableOLE1DDE = 0x4,
		SpeedOverMemory = 0x8
	}

	public enum WICDecodeOptions : uint
	{
		MetadataCacheOnDemand = 0,
		MetadataCacheOnLoad = 1
	}

	[DllImport("ole32.dll")]
	public static extern int CoInitializeEx(IntPtr pvReserved, CoInit dwCoInit);

	[DllImport("ole32.dll")]
	public static extern int CoCreateInstance(
		[In] ref Guid rclsid,
		[MarshalAs(UnmanagedType.IUnknown)] object? pUnkOuter,
		CLSCTX dwClsContext,
		[In] ref Guid riid,
		out IntPtr ppv);


	public static uint GetImageHeight(string path)
	{
		path = path.Replace(@"\\", @"\");

		var hr = CoInitializeEx(IntPtr.Zero, CoInit.MultiThreaded);
		if ( hr != 0 )
		{
			Marshal.ThrowExceptionForHR(hr);
		}

		IWICImagingFactory factory = null;
		IWICBitmapDecoder decoder = null;
		IWICBitmapFrameDecode frame = null;

		try
		{
			var clsid = CLSID.WICImagingFactory;
			var iid = IID.IWICImagingFactory;

			hr = CoCreateInstance(ref clsid, null, CLSCTX.INPROC_SERVER, ref iid,
				out var factoryPtr);
			if ( hr != 0 )
			{
				Marshal.ThrowExceptionForHR(hr);
			}

			factory = ( IWICImagingFactory ) Marshal.GetObjectForIUnknown(factoryPtr);

			var vendorGuid = Guid.Empty;
			factory.CreateDecoderFromFilename(path, ref vendorGuid, 0,
				WICDecodeOptions.MetadataCacheOnDemand, out decoder);

			decoder.GetFrame(0, out frame);
			frame.GetSize(out _, out var height);

			return height;
		}
		finally
		{
			if ( frame != null )
			{
				Marshal.ReleaseComObject(frame);
			}

			if ( decoder != null )
			{
				Marshal.ReleaseComObject(decoder);
			}

			if ( factory != null )
			{
				Marshal.ReleaseComObject(factory);
			}

			// Always safe to call even if it was already initialized
			Marshal.ReleaseComObject(factory);
		}
	}

	internal static class CLSID
	{
		internal static readonly Guid WICImagingFactory =
			new("CACAF262-9370-4615-A13B-9F5539DA4C0A");
	}

	internal static class IID
	{
		internal static readonly Guid IWICImagingFactory =
			new("EC5EC8A9-C395-4314-9C77-54D7A935FF70");
	}

	[ComImport]
	[Guid("EC5EC8A9-C395-4314-9C77-54D7A935FF70")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWICImagingFactory
	{
		void CreateDecoderFromFilename(
			[MarshalAs(UnmanagedType.LPWStr)] string wzFilename,
			[In] ref Guid pguidVendor,
			uint dwDesiredAccess,
			WICDecodeOptions metadataOptions,
			out IWICBitmapDecoder ppIDecoder);

		// other methods omitted
	}

	[ComImport]
	[Guid("9EDDE9E7-8DEE-47ea-99DF-E6FAF2ED44BF")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWICBitmapDecoder
	{
		void QueryCapability(); // simplified, skipped for brevity

		void GetMetadataQueryReader(); // skip

		void GetPreview();

		void GetColorContexts();

		void GetThumbnail();

		void GetFrameCount(out uint count);

		void GetFrame(uint index, out IWICBitmapFrameDecode frame);
	}

	[ComImport]
	[Guid("3B16811B-6A43-4ec9-A813-3D930C13B940")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWICBitmapFrameDecode
	{
		void GetSize(out uint width, out uint height);

		// other methods omitted
	}
}
