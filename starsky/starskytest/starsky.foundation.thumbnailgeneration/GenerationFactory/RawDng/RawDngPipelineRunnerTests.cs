using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class RawDngPipelineRunnerTests
{
	[TestMethod]
	public void Run_WithHooks_InvokesStepsInRequestedOrder()
	{
		var raw = new DngRawImage
		{
			Width = 2,
			Height = 2,
			BitsPerSample = 16,
			BlackLevel = new[] { 0f, 0f, 0f, 0f },
			WhiteLevel = new[] { 1024f, 1024f, 1024f, 1024f },
			AsShotNeutral = [1f, 1f, 1f],
			ColorMatrix1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CfaPattern = [0, 1, 1, 2],
			ForwardMatrix1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CameraCalibration1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CameraCalibration2 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CalibrationIlluminant1 = 17,
			Bayer = new ushort[,] { { 0, 512 }, { 1024, 1024 } }
		};

		var steps = new List<RawDngPipelineStep>();
		var state = RawDngPipelineExecutor.Run(raw, steps.Add);

		CollectionAssert.AreEqual(
			new[]
			{
				RawDngPipelineStep.DumpRawGrayscaleImage, RawDngPipelineStep.Normalize,
				RawDngPipelineStep.BilinearDemosaic, RawDngPipelineStep.WhiteBalance,
				RawDngPipelineStep.ColorMatrix, RawDngPipelineStep.ExposureCompensation,
				RawDngPipelineStep.ToneCurve
			}, steps);
		Assert.IsNotNull(state.DisplayRgb);
	}

	[TestMethod]
	public void TryRun_StreamPipeline_InvokesReadStepFirst()
	{
		using var dng = BuildMinimalDng();
		var steps = new List<RawDngPipelineStep>();

		var ok = RawDngPipelineRunner.TryRun(dng, out var state, out var error, steps.Add);

		Assert.IsTrue(ok, error);
		Assert.IsNotNull(state);
		Assert.IsGreaterThanOrEqualTo(2, steps.Count);
		Assert.AreEqual(RawDngPipelineStep.ReadTiff, steps[0]);
		Assert.AreEqual(RawDngPipelineStep.DumpRawGrayscaleImage, steps[1]);
	}

	[TestMethod]
	public void TryRunToJpeg_WritesJpegToOutputStream()
	{
		using var dng = BuildMinimalDng();
		using var output = new MemoryStream();

		var ok = RawDngPipelineRunner.TryRunToJpeg(dng, output, out var error);

		Assert.IsTrue(ok, error);
		Assert.IsGreaterThan(8, output.Length);
		var bytes = output.ToArray();
		Assert.AreEqual(( byte ) 0xFF, bytes[0]);
		Assert.AreEqual(( byte ) 0xD8, bytes[1]);
	}

	[TestMethod]
	public void TryRunToJpeg_WhenOnlyEmbeddedPreviewExists_DoesNotFallbackToPreview()
	{
		using var dng = BuildPreviewOnlyDng();
		using var output = new MemoryStream();

		var ok = RawDngPipelineRunner.TryRunToJpeg(dng, output, out var error);

		Assert.IsFalse(ok);
		Assert.AreEqual(0, output.Length);
		Assert.AreEqual("Missing width/height/bits metadata", error);
	}

	[TestMethod]
	public void ExecutorTryRunToJpeg_WhenOnlyEmbeddedPreviewExists_DoesNotFallbackToPreview()
	{
		using var dng = BuildPreviewOnlyDng();
		using var output = new MemoryStream();

		var ok = RawDngPipelineExecutor.TryRunToJpeg(dng, output, out var error);

		Assert.IsFalse(ok);
		Assert.AreEqual(0, output.Length);
		Assert.AreEqual("Missing width/height/bits metadata", error);
	}

	private static MemoryStream BuildMinimalDng()
	{
		var data = new byte[384];

		data[0] = ( byte ) 'I';
		data[1] = ( byte ) 'I';
		WriteU16(data, 2, 42);
		WriteU32(data, 4, 8);

		const int entryCount = 9;
		WriteU16(data, 8, entryCount);
		var entryBase = 10;
		const uint rawDataOffset = 220;

		WriteIfdEntry(data, entryBase, 0x0100, 4, 1, 2); // width
		WriteIfdEntry(data, entryBase + 12, 0x0101, 4, 1, 2); // height
		WriteIfdEntry(data, entryBase + 24, 0x0102, 3, 1, 16); // bits
		WriteIfdEntry(data, entryBase + 36, 0x0103, 3, 1, 1); // compression
		WriteIfdEntry(data, entryBase + 48, 0x0106, 3, 1, 32803); // CFA photometric
		WriteIfdEntry(data, entryBase + 60, 0x0111, 4, 1, rawDataOffset); // strip offset
		WriteIfdEntry(data, entryBase + 72, 0x0117, 4, 1, 8); // strip bytes
		WriteIfdEntry(data, entryBase + 84, 0x828E, 1, 4, 0x02010100); // RGGB
		WriteIfdEntry(data, entryBase + 96, 0xC61D, 3, 1, 1024); // white level
		WriteU32(data, entryBase + entryCount * 12, 0);

		WriteU16(data, ( int ) rawDataOffset + 0, 100);
		WriteU16(data, ( int ) rawDataOffset + 2, 200);
		WriteU16(data, ( int ) rawDataOffset + 4, 300);
		WriteU16(data, ( int ) rawDataOffset + 6, 400);

		return new MemoryStream(data);
	}

	private static MemoryStream BuildPreviewOnlyDng()
	{
		var data = new byte[256];

		data[0] = ( byte ) 'I';
		data[1] = ( byte ) 'I';
		WriteU16(data, 2, 42);
		WriteU32(data, 4, 8);

		const uint subIfdOffset = 64;
		const uint jpegOffset = 200;
		WriteU16(data, 8, 1);
		WriteIfdEntry(data, 10, 0x014A, 4, 1, subIfdOffset);
		WriteU32(data, 22, 0);

		WriteU16(data, ( int ) subIfdOffset, 6);
		var entryBase = ( int ) subIfdOffset + 2;
		WriteIfdEntry(data, entryBase + 0, 0x0100, 4, 1, 1);
		WriteIfdEntry(data, entryBase + 12, 0x0101, 4, 1, 1);
		WriteIfdEntry(data, entryBase + 24, 0x0103, 3, 1, 7);
		WriteIfdEntry(data, entryBase + 36, 0x0106, 3, 1, 2);
		WriteIfdEntry(data, entryBase + 48, 0x0111, 4, 1, jpegOffset);
		WriteIfdEntry(data, entryBase + 60, 0x0117, 4, 1, 4);
		WriteU32(data, entryBase + 72, 0);

		data[jpegOffset + 0] = 0xFF;
		data[jpegOffset + 1] = 0xD8;
		data[jpegOffset + 2] = 0xFF;
		data[jpegOffset + 3] = 0xD9;

		return new MemoryStream(data);
	}

	private static void WriteIfdEntry(byte[] data, int offset, ushort tag, ushort type,
		uint count, uint value)
	{
		WriteU16(data, offset, tag);
		WriteU16(data, offset + 2, type);
		WriteU32(data, offset + 4, count);
		WriteU32(data, offset + 8, value);
	}

	private static void WriteU16(byte[] data, int offset, ushort value)
	{
		data[offset] = ( byte ) ( value & 0xFF );
		data[offset + 1] = ( byte ) ( ( value >> 8 ) & 0xFF );
	}

	private static void WriteU32(byte[] data, int offset, uint value)
	{
		data[offset] = ( byte ) ( value & 0xFF );
		data[offset + 1] = ( byte ) ( ( value >> 8 ) & 0xFF );
		data[offset + 2] = ( byte ) ( ( value >> 16 ) & 0xFF );
		data[offset + 3] = ( byte ) ( ( value >> 24 ) & 0xFF );
	}
}
