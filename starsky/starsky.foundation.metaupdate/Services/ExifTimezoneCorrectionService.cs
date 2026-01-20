using System.Diagnostics.CodeAnalysis;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
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
		ExifTimezoneCorrectionRequest request)
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

	public async Task<List<ExifTimezoneCorrectionResult>> Validate(string[] subPaths,
		bool collections, ExifTimezoneCorrectionRequest request)
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
		ExifTimezoneCorrectionRequest request)
	{
		var result = ValidateCorrection(fileIndexItem, request);
		if ( !string.IsNullOrEmpty(result.Error) )
		{
			return result;
		}

		try
		{
			// Calculate the timezone delta
			var delta = CalculateTimezoneDelta(
				fileIndexItem.DateTime,
				request.RecordedTimezone,
				request.CorrectTimezone);

			result.OriginalDateTime = fileIndexItem.DateTime;
			result.DeltaHours = delta.TotalHours;

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
				$"(delta: {result.DeltaHours:F2}h)");
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
		ExifTimezoneCorrectionRequest request)
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

		// Validate timezones
		if ( string.IsNullOrWhiteSpace(request.RecordedTimezone) )
		{
			fileIndexItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			result.Error = "Recorded timezone is required";
			return result;
		}

		if ( string.IsNullOrWhiteSpace(request.CorrectTimezone) )
		{
			fileIndexItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			result.Error = "Correct timezone is required";
			return result;
		}

		// Validate timezone IDs
		try
		{
			_ = TimeZoneInfo.FindSystemTimeZoneById(request.RecordedTimezone);
		}
		catch ( Exception )
		{
			fileIndexItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			result.Error = $"Invalid recorded timezone: {request.RecordedTimezone}";
			return result;
		}

		try
		{
			_ = TimeZoneInfo.FindSystemTimeZoneById(request.CorrectTimezone);
		}
		catch ( Exception )
		{
			fileIndexItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			result.Error = $"Invalid correct timezone: {request.CorrectTimezone}";
			return result;
		}

		// Validate DateTime
		if ( fileIndexItem.DateTime.Year < 2 )
		{
			fileIndexItem.Status = FileIndexItem.ExifStatus.OperationNotSupported;
			result.Error = "Image does not have a valid DateTime in EXIF";
			return result;
		}


		// Calculate delta to check for day rollover
		var delta = CalculateTimezoneDelta(
			fileIndexItem.DateTime,
			request.RecordedTimezone,
			request.CorrectTimezone);

		var correctedDateTime = fileIndexItem.DateTime.Add(delta);

		// Warn about day/month/year rollover
		if ( correctedDateTime.Day != fileIndexItem.DateTime.Day )
		{
			result.Warning =
				$"Correction will change the day from " +
				$"{fileIndexItem.DateTime:yyyy-MM-dd} to {correctedDateTime:yyyy-MM-dd}";
		}

		// Warn if timezones are the same
		if ( request.RecordedTimezone == request.CorrectTimezone )
		{
			fileIndexItem.Status = FileIndexItem.ExifStatus.OkAndSame;
			result.Warning = "Recorded and correct timezones are the same - no correction needed";
			return result;
		}

		fileIndexItem.Status = FileIndexItem.ExifStatus.Ok;
		return result;
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
