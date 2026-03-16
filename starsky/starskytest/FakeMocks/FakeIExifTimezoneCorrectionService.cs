using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starskytest.FakeMocks;

/// <summary>
///     Fake implementation of IExifTimezoneCorrectionService for testing
/// </summary>
internal sealed class FakeIExifTimezoneCorrectionService(
	IUpdateBackgroundTaskQueue queue,
	List<ExifTimezoneCorrectionResult>? validationResults = null
)
	: IExifTimezoneCorrectionService
{
	private readonly List<ExifTimezoneCorrectionResult>
		_validationResults = validationResults ?? [];


	public Task<List<ExifTimezoneCorrectionResult>> Validate(string f, bool collections,
		IExifTimeCorrectionRequest request)
	{
		return Task.FromResult(_validationResults);
	}

	public Task<List<ExifTimezoneCorrectionResult>> Validate(string[] subPaths, bool collections,
		IExifTimeCorrectionRequest request)
	{
		return Task.FromResult(_validationResults);
	}

	public Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
		List<FileIndexItem> fileIndexItems, IExifTimeCorrectionRequest request)
	{
		return Task.FromResult(_validationResults);
	}

	public async Task QueueCorrectionTask(List<ExifTimezoneCorrectionResult> validateResults,
		IExifTimeCorrectionRequest request,
		string correctionType)
	{
		var sut = new ExifTimezoneCorrectionService(queue,
			new FakeExifTool(new FakeIStorage(), new AppSettings()),
			new FakeSelectorStorage(), new FakeIQuery(), new FakeIThumbnailQuery(),
			new AppSettings(), new FakeIWebLogger());

		await sut.QueueCorrectionTask(validateResults, request, correctionType);
	}
}
