using System.Runtime.InteropServices;

namespace starsky.foundation.native.PreviewImageNative.Helpers;

public static class ImageMetadataReaderWindowsBindings
{
	public static int GetImageHeight(string imagePath)
	{
		IWICImagingFactory? factory = null;
		IWICBitmapDecoder? decoder = null;
		IWICBitmapFrameDecode? frame = null;

		try
		{
			// Create WIC factory
			factory = ( IWICImagingFactory ) new WICImagingFactory();

			// Create decoder for the image
			factory.CreateDecoderFromFilename(imagePath, IntPtr.Zero, 0,
				WICDecodeOptions.WICDecodeMetadataCacheOnDemand, out decoder);

			// Get the first frame
			decoder.GetFrame(0, out frame);

			// Get image height
			frame.GetSize(out _, out var height);
			return ( int ) height;
		}
		finally
		{
			// Release COM objects
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
		}
	}

	[ComImport]
	[Guid("ec5ec8a9-c395-4314-9c77-54d7a935ff70")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IWICImagingFactory
	{
		void CreateDecoderFromFilename(
			[MarshalAs(UnmanagedType.LPWStr)] string wzFilename,
			IntPtr pguidVendor,
			uint dwDesiredAccess,
			WICDecodeOptions metadataOptions,
			out IWICBitmapDecoder ppIDecoder);
	}

	[ComImport]
	[Guid("9EDDE9E7-8DEE-47ea-99DF-E6FAF2ED44BF")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IWICBitmapDecoder
	{
		void GetFrame(uint index, out IWICBitmapFrameDecode ppIBitmapFrame);
	}

	[ComImport]
	[Guid("3B16811B-6A43-4ec9-A813-3D930C13B940")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IWICBitmapFrameDecode
	{
		void GetSize(out uint puiWidth, out uint puiHeight);
	}

	[ComImport]
	[Guid("CACAF262-9370-4615-A13B-9F5539DA4C0A")]
	private class WICImagingFactory
	{
	}

	private enum WICDecodeOptions
	{
		WICDecodeMetadataCacheOnDemand = 0,
		WICDecodeMetadataCacheOnLoad = 1
	}
}
