using System.IO;
using BenchmarkDotNet.Attributes;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starsky.benchmarks.rawdng;

[MemoryDiagnoser]
[InProcess]
public class RawDngLoadBenchmarks
{
	private byte[] _dng8Bit = [];
	private byte[] _dng14BitPacked = [];
	private byte[] _dng16Bit = [];

	[Params(4032, 6048)]
	public int Width { get; set; }

	[Params(3024, 4024)]
	public int Height { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		_dng8Bit = MinimalDngFactory.CreateUncompressedCfaDng(Width, Height, bitsPerSample: 8);
		_dng14BitPacked = MinimalDngFactory.CreateUncompressedCfaDng(Width, Height, bitsPerSample: 14, packed: true);
		_dng16Bit = MinimalDngFactory.CreateUncompressedCfaDng(Width, Height, bitsPerSample: 16);
	}

	[Benchmark]
	public int TryLoad_8Bit()
	{
		using var stream = new MemoryStream(_dng8Bit, writable: false);
		return TryLoadAndCountPixels(stream);
	}

	[Benchmark]
	public int TryLoad_14Bit_Packed()
	{
		using var stream = new MemoryStream(_dng14BitPacked, writable: false);
		return TryLoadAndCountPixels(stream);
	}

	[Benchmark]
	public int TryLoad_16Bit()
	{
		using var stream = new MemoryStream(_dng16Bit, writable: false);
		return TryLoadAndCountPixels(stream);
	}

	private static int TryLoadAndCountPixels(Stream input)
	{
		var ok = DngSubsetReader.TryLoad(input, out var image, out var error);
		if ( !ok || image == null )
		{
			throw new InvalidDataException(error);
		}

		return image.Width * image.Height;
	}
}

