using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;

namespace starskytest.FakeMocks;

public class FakeIFfMpegDownloadBinaries : IFfMpegDownloadBinaries
{
	private readonly FfmpegDownloadStatus? _status;

	public FakeIFfMpegDownloadBinaries(FfmpegDownloadStatus? status = null)
	{
		_status = status;
	}

	public Task<FfmpegDownloadStatus> Download(
		KeyValuePair<BinaryIndex?, List<Uri>> binaryIndexKeyValuePair, string currentArchitecture,
		int retryInSeconds = 15)
	{
		return Task.FromResult(_status ?? FfmpegDownloadStatus.Ok);
	}
}
