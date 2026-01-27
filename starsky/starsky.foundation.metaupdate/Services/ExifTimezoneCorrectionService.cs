using System.Diagnostics.CodeAnalysis;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.writemeta.Helpers;
using starsky.foundation.writemeta.Interfaces;

namespace starsky.foundation.metaupdate.Services;

/// <summary>
///     Implementation of EXIF timezone correction service
/// </summary>
[Service(typeof(IExifTimezoneCorrectionService),
	InjectionLifetime = InjectionLifetime.Scoped)]
public class ExifTimezoneCorrectionService : IExifTimezoneCorrectionService
{
	private readonly AppSettings _appSettings;
	private readonly ExifToolCmdHelper _exifToolCmdHelper;
	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly IStorage _storage;

	public ExifTimezoneCorrectionService(
		IExifTool exifTool,
		ISelectorStorage selectorStorage,
		IQuery query,
		IThumbnailQuery thumbnailQuery,
		AppSettings appSettings,
		IWebLogger logger)
	{
		_storage = selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
		_exifToolCmdHelper = new ExifToolCmdHelper(exifTool,
			_storage,
			selectorStorage.Get(SelectorStorage.StorageServices.Thumbnail),
			new ReadMeta(_storage, appSettings, null, logger),
			thumbnailQuery, logger);
		_logger = logger;
		_query = query;
		_appSettings = appSettings;
	}

	/// <summary>
	///     Correct EXIF timestamps for multiple images
	/// </summary>
	[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
	public async Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
		List<FileIndexItem> fileIndexItems,
		IExifTimeCorrectionRequest request)
	{
		var results = new List<ExifTimezoneCorrectionResult>();

		// keep for because of editing the list in the loop
		for ( var i = 0; i < fileIndexItems.Count; i++ )
		{
			var result = await CorrectTimezoneAsync(fileIndexItems[i], request);
			results.Add(result);
		}

		return results;
	}

	public async Task<List<ExifTimezoneCorrectionResult>> Validate(string f, bool collections,
		IExifTimeCorrectionRequest request)
	{
		var subPaths = PathHelper.SplitInputFilePaths(f);
		return await Validate(subPaths, collections, request);
	}

	public async Task<List<ExifTimezoneCorrectionResult>> Validate(string[] subPaths,
		bool collections, IExifTimeCorrectionRequest request)
	{
		var fileIndexItems = await _query.GetObjectsByFilePathAsync(
			[..subPaths], collections);

		var results =
			fileIndexItems.Select(fileIndexItem =>
					ValidateCorrection(fileIndexItem,
						request))
				.ToList();
		return results;
	}

	/// <summary>
	///     Correct EXIF timestamps for a single image
	/// </summary>
	internal async Task<ExifTimezoneCorrectionResult> CorrectTimezoneAsync(
		FileIndexItem fileIndexItem,
		IExifTimeCorrectionRequest request)
	{
		var result = ValidateCorrection(fileIndexItem, request);
		if ( !string.IsNullOrEmpty(result.Error) )
		{
			return result;
		}

		try
		{
			// Calculate the timezone or custom offset delta

			var delta = request switch
			{
				ExifTimezoneBasedCorrectionRequest timezoneRequest => CalculateTimezoneDelta(
					fileIndexItem.DateTime, timezoneRequest.RecordedTimezone,
					timezoneRequest.CorrectTimezone),
				ExifCustomOffsetCorrectionRequest customOffsetRequest => CalculateCustomOffsetDelta(
					fileIndexItem.DateTime, customOffsetRequest),
				_ => throw new ArgumentException("Invalid request type", nameof(request))
			};

			result.OriginalDateTime = fileIndexItem.DateTime;
			result.Delta = delta;

			// Apply the correction
			var correctedDateTime = fileIndexItem.DateTime.Add(delta);
			result.CorrectedDateTime = correctedDateTime;

			// Update the FileIndexItem with corrected DateTime
			fileIndexItem.DateTime = correctedDateTime;
			fileIndexItem.LastEdited = _storage.Info(fileIndexItem.FilePath!).LastWriteTime;

			// to avoid diskWatcher catch up
			_query.SetGetObjectByFilePathCache(fileIndexItem.FilePath!, fileIndexItem,
				TimeSpan.FromSeconds(5));

			// Write the corrected DateTime to EXIF
			var comparedNames =
				new List<string> { nameof(FileIndexItem.DateTime).ToLowerInvariant() };
			await _exifToolCmdHelper.UpdateAsync(
				fileIndexItem,
				comparedNames,
				false);

			var fileHashService = new FileHash(_storage, _logger);
			var newFileHash = ( await fileHashService.GetHashCodeAsync(
				fileIndexItem.FilePath!,
				fileIndexItem.ImageFormat) ).Key;
			fileIndexItem.FileHash = newFileHash;

			await _query.UpdateItemAsync(fileIndexItem);

			result.Success = true;
			_logger.LogInformation(
				$"[ExifTimezoneCorrection] Successfully corrected: {fileIndexItem.FilePath} " +
				$"from {result.OriginalDateTime:yyyy-MM-dd HH:mm:ss} " +
				$"to {result.CorrectedDateTime:yyyy-MM-dd HH:mm:ss} " +
				$"(delta: {result.Delta.Hours:F2}h)");
		}
		catch ( Exception ex )
		{
			result.Success = false;
			result.Error = $"Exception during correction: {ex.Message}";
			_logger.LogError(
				$"[ExifTimezoneCorrection] Exception: {fileIndexItem.FilePath} - {ex.Message}", ex);
		}

		return result;
	}

	/// <summary>
	///     Validate timezone correction request
	/// </summary>
	internal ExifTimezoneCorrectionResult ValidateCorrection(
		FileIndexItem fileIndexItem,
		IExifTimeCorrectionRequest request)
	{
		var result = new ExifTimezoneCorrectionResult
		{
			Success = false,
			OriginalDateTime = fileIndexItem.DateTime,
			FileIndexItem = fileIndexItem
		};

		if ( !_storage.ExistFile(fileIndexItem.FilePath!) )
		{
			result.Error = "File does not exist";
			fileIndexItem.Status = FileIndexItem.ExifStatus.NotFoundSourceMissing;
			return result;
		}

		// Dir is readonly / don't edit
		if ( new StatusCodesHelper(_appSettings).IsReadOnlyStatus(fileIndexItem)
		     == FileIndexItem.ExifStatus.ReadOnly )
		{
			fileIndexItem.Status = FileIndexItem.ExifStatus.ReadOnly;
			result.Error = "Directory is read only";
			return result;
		}

		// Validate DateTime
		if ( fileIndexItem.DateTime.Year < 2 )
		{
			fileIndexItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			result.Error = "Image does not have a valid DateTime in EXIF";
			return result;
		}

		// Determine mode and validate accordingly
		switch ( request )
		{
			case ExifCustomOffsetCorrectionRequest customOffsetRequest:
				var customOffsetValidation =
					ValidateCustomOffsetRequest(fileIndexItem, customOffsetRequest, result);
				if ( !customOffsetValidation.Success )
				{
					return customOffsetValidation;
				}

				result.Delta =
					CalculateCustomOffsetDelta(fileIndexItem.DateTime, customOffsetRequest);
				break;
			case ExifTimezoneBasedCorrectionRequest timezoneRequest:
				var timezoneValidation =
					ValidateTimezoneRequest(fileIndexItem, timezoneRequest, result);
				if ( !timezoneValidation.Success )
				{
					return timezoneValidation;
				}

				if ( timezoneRequest.RecordedTimezone == timezoneRequest.CorrectTimezone )
				{
					fileIndexItem.Status = FileIndexItem.ExifStatus.OkAndSame;
					result.Warning =
						"Recorded and correct timezones are the same - no correction needed";
					result.Delta = TimeSpan.Zero;
					result.CorrectedDateTime = fileIndexItem.DateTime;
					result.Success = true;
					return result;
				}

				result.Delta = CalculateTimezoneDelta(fileIndexItem.DateTime,
					timezoneRequest.RecordedTimezone, timezoneRequest.CorrectTimezone);
				break;
			default:
				throw new ArgumentException("Invalid request type", nameof(request));
		}

		result.CorrectedDateTime = fileIndexItem.DateTime.Add(result.Delta);
		result.Success = true;

		// Warn about day/month/year rollover
		if ( result.CorrectedDateTime.Day != fileIndexItem.DateTime.Day )
		{
			result.Warning =
				$"Correction will change the day from " +
				$"{fileIndexItem.DateTime:yyyy-MM-dd} to " +
				$"{result.CorrectedDateTime:yyyy-MM-dd}";
		}

		// Warn if no actual change (delta is zero)
		if ( result.Delta == TimeSpan.Zero )
		{
			result.Warning = "No time correction will be applied (delta is zero)";
		}

		fileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
		return result;
	}

	private ExifTimezoneCorrectionResult SetError(ExifTimezoneCorrectionResult result,
		FileIndexItem fileIndexItem, FileIndexItem.ExifStatus status, string error)
	{
		fileIndexItem.Status = status;
		result.Error = error;
		return result;
	}

	private ExifTimezoneCorrectionResult ValidateCustomOffsetRequest(FileIndexItem fileIndexItem,
		ExifCustomOffsetCorrectionRequest request, ExifTimezoneCorrectionResult result)
	{
		if ( !request.HasAnyOffset )
		{
			return SetError(result, fileIndexItem, FileIndexItem.ExifStatus.OperationNotSupported,
				"At least one custom offset value is required");
		}

		result.Success = true;
		return result;
	}

	private ExifTimezoneCorrectionResult ValidateTimezoneRequest(FileIndexItem fileIndexItem,
		ExifTimezoneBasedCorrectionRequest request, ExifTimezoneCorrectionResult result)
	{
		if ( string.IsNullOrWhiteSpace(request.RecordedTimezone) )
		{
			return SetError(result, fileIndexItem, FileIndexItem.ExifStatus.OperationNotSupported,
				"Recorded timezone is required");
		}

		if ( string.IsNullOrWhiteSpace(request.CorrectTimezone) )
		{
			return SetError(result, fileIndexItem, FileIndexItem.ExifStatus.OperationNotSupported,
				"Correct timezone is required");
		}

		try
		{
			_ = TimeZoneInfo.FindSystemTimeZoneById(request.RecordedTimezone);
		}
		catch ( Exception )
		{
			return SetError(result, fileIndexItem, FileIndexItem.ExifStatus.OperationNotSupported,
				$"Invalid recorded timezone: {request.RecordedTimezone}");
		}

		try
		{
			_ = TimeZoneInfo.FindSystemTimeZoneById(request.CorrectTimezone);
		}
		catch ( Exception )
		{
			return SetError(result, fileIndexItem, FileIndexItem.ExifStatus.OperationNotSupported,
				$"Invalid correct timezone: {request.CorrectTimezone}");
		}

		result.Success = true;
		return result;
	}

	/// <summary>
	///     Calculate custom offset delta from request parameters
	///     Supports years, months, days, hours, minutes, and seconds
	/// </summary>
	/// <param name="dateTime">The base datetime to apply offsets to</param>
	/// <param name="request">The request containing custom offset values</param>
	/// <returns>TimeSpan delta to apply (for time components) or modified DateTime (for date components)</returns>
	private static TimeSpan CalculateCustomOffsetDelta(
		DateTime dateTime,
		ExifCustomOffsetCorrectionRequest request)
	{
		// For date components (years, months, days), we need to calculate the difference
		// by applying them to the datetime first, then getting the TimeSpan difference
		var targetDateTime = dateTime;

		// Apply year and month offsets using AddYears/AddMonths
		if ( request.Year.HasValue )
		{
			targetDateTime = targetDateTime.AddYears(request.Year.Value);
		}

		if ( request.Month.HasValue )
		{
			targetDateTime = targetDateTime.AddMonths(request.Month.Value);
		}

		// Calculate the difference after date adjustments
		var delta = targetDateTime - dateTime;

		// Add time-based offsets
		if ( request.Day.HasValue )
		{
			delta = delta.Add(TimeSpan.FromDays(request.Day.Value));
		}

		if ( request.Hour.HasValue )
		{
			delta = delta.Add(TimeSpan.FromHours(request.Hour.Value));
		}

		if ( request.Minute.HasValue )
		{
			delta = delta.Add(TimeSpan.FromMinutes(request.Minute.Value));
		}

		if ( request.Second.HasValue )
		{
			delta = delta.Add(TimeSpan.FromSeconds(request.Second.Value));
		}

		return delta;
	}

	/// <summary>
	///     Calculate the timezone offset delta between recorded and correct timezones
	///     This method is DST-aware and calculates offsets based on the actual date
	/// </summary>
	/// <param name="dateTime">The datetime to calculate offsets for</param>
	/// <param name="recordedTimezone">Source timezone (what camera thought)</param>
	/// <param name="correctTimezone">Target timezone (actual location)</param>
	/// <returns>TimeSpan delta to apply</returns>
	private static TimeSpan CalculateTimezoneDelta(
		DateTime dateTime,
		string recordedTimezone,
		string correctTimezone)
	{
		// Parse timezones
		var recordedTz = TimeZoneInfo.FindSystemTimeZoneById(recordedTimezone);
		var correctTz = TimeZoneInfo.FindSystemTimeZoneById(correctTimezone);

		// EXIF datetime is naive local time - we need to treat it as if it were in the recorded timezone
		// Then convert it to the correct timezone

		// Get the offset for the recorded timezone at this specific date (handles DST)
		var recordedOffset = recordedTz.GetUtcOffset(dateTime);

		// Get the offset for the correct timezone at this specific date (handles DST)
		var correctOffset = correctTz.GetUtcOffset(dateTime);

		// Calculate delta: to get from recorded to correct
		// If photo was taken at 14:00 in GMT+02 but camera stored it as GMT+00,
		// we need to add 2 hours
		var delta = correctOffset - recordedOffset;

		return delta;
	}
}
