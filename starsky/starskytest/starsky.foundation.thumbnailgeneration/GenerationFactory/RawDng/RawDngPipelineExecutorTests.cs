using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;
using System.Collections.Generic;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class RawDngPipelineExecutorTests
{
	[TestMethod]
	public void Run_ProducesNormalizedAndLinearRgb_WithWhiteBalanceApplied()
	{
		var raw = new DngRawImage
		{
			Width = 2,
			Height = 2,
			BitsPerSample = 16,
			BlackLevel = new[] { 0f, 0f, 0f, 0f },
			WhiteLevel = new[] { 1024f, 1024f, 1024f, 1024f },
			AsShotNeutral = new[] { 2f, 1f, 4f },
			ColorMatrix1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CfaPattern = new byte[] { 0, 1, 1, 2 },
			ForwardMatrix1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CameraCalibration1 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CameraCalibration2 = new[,] { { 1f, 0f, 0f }, { 0f, 1f, 0f }, { 0f, 0f, 1f } },
			CalibrationIlluminant1 = 17, // D50
			Bayer = new ushort[,]
			{
				{ 0, 512 },
				{ 1024, 1024 }
			}
		};

		var state = RawDngPipelineExecutor.Run(raw);

		Assert.IsNotNull(state.NormalizedBayer);
		Assert.IsNotNull(state.LinearRgb);
		Assert.IsNotNull(state.DisplayRgb);
		Assert.IsNotNull(state.WhiteBalanceGains);
		Assert.IsNotNull(state.CameraToSrgbMatrix);
		Assert.AreEqual(0f, state.NormalizedBayer[0, 0], 1e-6f);
		Assert.AreEqual(0.5f, state.NormalizedBayer[0, 1], 1e-6f);
		Assert.AreEqual(1f, state.NormalizedBayer[1, 0], 1e-6f);
		Assert.AreEqual(1f, state.NormalizedBayer[1, 1], 1e-6f);

		Assert.AreEqual(0.5f, state.WhiteBalanceGains[0], 1e-6f);
		Assert.AreEqual(1f, state.WhiteBalanceGains[1], 1e-6f);
		Assert.AreEqual(0.25f, state.WhiteBalanceGains[2], 1e-6f);

		// Color matrix mixes channels, so site-local assumptions no longer hold.
		Assert.IsFalse(float.IsNaN(state.LinearRgb[0, 0, 0]));
		Assert.IsFalse(float.IsNaN(state.LinearRgb[1, 1, 2]));
		Assert.AreNotEqual(0.25f, state.LinearRgb[1, 1, 2], 1e-6f);

		Assert.IsTrue(state.DisplayRgb[0, 0, 0] is >= 0f and <= 1f);
		Assert.IsTrue(state.DisplayRgb[1, 1, 2] is >= 0f and <= 1f);
	}

	[TestMethod]
	public void Run_WithStepHook_UsesCanonicalRawProcessingOrder()
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
			CalibrationIlluminant1 = 21,
			Bayer = new ushort[,] { { 0, 512 }, { 1024, 1024 } }
		};

		var steps = new List<RawDngPipelineStep>();
		RawDngPipelineExecutor.Run(raw, steps.Add);

		CollectionAssert.AreEqual(
			new[]
			{
				RawDngPipelineStep.DumpRawGrayscaleImage, RawDngPipelineStep.Normalize,
				RawDngPipelineStep.BilinearDemosaic, RawDngPipelineStep.WhiteBalance,
				RawDngPipelineStep.ColorMatrix, RawDngPipelineStep.ExposureCompensation,
				RawDngPipelineStep.ToneCurve
			}, steps);
	}

	[TestMethod]
	public void Run_WithOrientationRotate90Clockwise_RotatesDisplayBuffer()
	{
		var raw = new DngRawImage
		{
			Width = 4,
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
			CalibrationIlluminant1 = 21,
			Orientation = 6,
			Bayer = new ushort[,]
			{
				{ 100, 200, 300, 400 },
				{ 500, 600, 700, 800 }
			}
		};

		var state = RawDngPipelineExecutor.Run(raw);
		Assert.IsNotNull(state.DisplayRgb);
		Assert.AreEqual(4, state.DisplayRgb.GetLength(0));
		Assert.AreEqual(2, state.DisplayRgb.GetLength(1));
	}
}

