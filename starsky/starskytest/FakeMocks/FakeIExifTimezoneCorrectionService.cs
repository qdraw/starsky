using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;

namespace starskytest.FakeMocks;

/// <summary>
///     Fake implementation of IExifTimezoneCorrectionService for testing
/// </summary>
internal sealed class FakeIExifTimezoneCorrectionService(
	List<ExifTimezoneCorrectionResult>? validationResults = null)
	: IExifTimezoneCorrectionService
{
	private readonly List<ExifTimezoneCorrectionResult>
		_validationResults = validationResults ?? [];

	public Task<List<ExifTimezoneCorrectionResult>> Validate(
		string[] subPaths,
		bool collections,
		ExifTimezoneCorrectionRequest request)
	{
		return Task.FromResult(_validationResults);
	}

	public Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
		List<FileIndexItem> fileIndexItems,
		ExifTimezoneCorrectionRequest request)
	{
		return Task.FromResult(_validationResults);
	}
}
