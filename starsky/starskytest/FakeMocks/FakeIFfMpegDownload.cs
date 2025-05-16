using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Interfaces;
using starsky.foundation.video.GetDependencies.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnZipfileFakeFFMpeg;

namespace starskytest.FakeMocks;

public class FakeIFfMpegDownload : IFfMpegDownload
{
	private FfmpegDownloadStatus _status = FfmpegDownloadStatus.Ok;

	public async Task<List<FfmpegDownloadStatus>> DownloadFfMpeg(List<string> architectures)
	{
		var result = new List<FfmpegDownloadStatus>();
		foreach ( var architecture in architectures )
		{
			result.Add(await DownloadFfMpeg(architecture));
		}

		return result;
	}

	public async Task<FfmpegDownloadStatus> DownloadFfMpeg(string? architecture = null)
	{
		if ( _status != FfmpegDownloadStatus.Ok )
		{
			Console.WriteLine("FfMpegDownload failed");
			return _status;
		}

		var hostStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());

		if ( new AppSettings().IsWindows )
		{
			var zipper = Zipper.ExtractZip([
				..
				new CreateAnZipfileFakeFfMpeg().Bytes
			]);

			var ffmpegExe = new MemoryStream(zipper.FirstOrDefault(p =>
				p.Key == "ffmpeg.exe").Value);
			await hostStorage.WriteStreamAsync(ffmpegExe,
				new CreateAnImage().BasePath + "ffmpeg.exe");
		}
		else
		{
			CreateStubFile(hostStorage, new CreateAnImage().BasePath + "ffmpeg",
				"#!/bin/bash\necho Fake Executable");

			await new FfMpegChmod(new FakeSelectorStorage(hostStorage),
					new FakeIWebLogger())
				.Chmod(
					new CreateAnImage().BasePath + "ffmpeg");
		}

		return _status;
	}

	public string GetSetFfMpegPath()
	{
		if ( new AppSettings().IsWindows )
		{
			return new CreateAnImage().BasePath + "ffmpeg.exe";
		}

		return new CreateAnImage().BasePath + "ffmpeg";
	}

	private static void CreateStubFile(StorageHostFullPathFilesystem storage, string path,
		string content)
	{
		var stream = StringToStreamHelper.StringToStream(content);
		storage.WriteStream(stream, path);
	}

	public void SetDownloadStatus(FfmpegDownloadStatus status)
	{
		_status = status;
	}

	public static void CleanUp()
	{
		File.Delete(new FakeIFfMpegDownload().GetSetFfMpegPath());
	}
}
