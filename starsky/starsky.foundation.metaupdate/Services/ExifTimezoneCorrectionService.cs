using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.readmeta.Interfaces;
using starsky.foundation.writemeta.Helpers;

namespace starsky.foundation.metaupdate.Services;

/// <summary>
/// Service to correct EXIF timestamps for images recorded in the wrong timezone
/// </summary>
public interface IExifTimezoneCorrectionService
{
	/// <summary>
	/// Correct EXIF timestamps for a single image
	/// </summary>
	/// <param name="fileIndexItem">The image to correct</param>
	/// <param name="request">Timezone correction parameters</param>
	/// <returns>Result of the correction operation</returns>
	Task<ExifTimezoneCorrectionResult> CorrectTimezoneAsync(
		FileIndexItem fileIndexItem,
		ExifTimezoneCorrectionRequest request);

	/// <summary>
	/// Correct EXIF timestamps for multiple images
	/// </summary>
	/// <param name="fileIndexItems">The images to correct</param>
	/// <param name="request">Timezone correction parameters</param>
	/// <returns>Results for each image</returns>
	Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
		List<FileIndexItem> fileIndexItems,
		ExifTimezoneCorrectionRequest request);

	/// <summary>
	/// Validate timezone correction request
	/// </summary>
	/// <param name="fileIndexItem">The image to validate</param>
	/// <param name="request">Timezone correction parameters</param>
	/// <returns>Validation result with warnings</returns>
	ExifTimezoneCorrectionResult ValidateCorrection(
		FileIndexItem fileIndexItem,
		ExifTimezoneCorrectionRequest request);
}

/// <summary>
/// Implementation of EXIF timezone correction service
/// </summary>
public class ExifTimezoneCorrectionService : IExifTimezoneCorrectionService
{
	private readonly ExifToolCmdHelper _exifToolCmdHelper;
	private readonly IWebLogger _logger;

	public ExifTimezoneCorrectionService(
		IReadMeta readMeta,
		ExifToolCmdHelper exifToolCmdHelper,
		IWebLogger logger)
	{
		_exifToolCmdHelper = exifToolCmdHelper;
		_logger = logger;
	}

	/// <summary>
	/// Correct EXIF timestamps for a single image
	/// </summary>
	public async Task<ExifTimezoneCorrectionResult> CorrectTimezoneAsync(
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

			// Write the corrected DateTime to EXIF
			var comparedNames = new List<string> { nameof(FileIndexItem.DateTime).ToLowerInvariant() };
			var writeResult = await _exifToolCmdHelper.UpdateAsync(
				fileIndexItem,
				comparedNames,
				includeSoftware: false);

			// Check if write was successful by verifying the command was executed
			result.Success = !string.IsNullOrEmpty(writeResult.Command) && 
			                 writeResult.Command != "-json -overwrite_original";

			if ( !result.Success )
			{
				result.Error = "Failed to write EXIF data";
				_logger.LogInformation($"[ExifTimezoneCorrection] Failed to write: {fileIndexItem.FilePath}");
			}
			else
			{
				_logger.LogInformation($"[ExifTimezoneCorrection] Successfully corrected: {fileIndexItem.FilePath} " +
				                       $"from {result.OriginalDateTime:yyyy-MM-dd HH:mm:ss} to {result.CorrectedDateTime:yyyy-MM-dd HH:mm:ss} " +
				                       $"(delta: {result.DeltaHours:F2}h)");
			}
		}
		catch ( Exception ex )
		{
			result.Success = false;
			result.Error = $"Exception during correction: {ex.Message}";
			_logger.LogError($"[ExifTimezoneCorrection] Exception: {fileIndexItem.FilePath} - {ex.Message}", ex);
		}

		return result;
	}

	/// <summary>
	/// Correct EXIF timestamps for multiple images
	/// </summary>
	public async Task<List<ExifTimezoneCorrectionResult>> CorrectTimezoneAsync(
		List<FileIndexItem> fileIndexItems,
		ExifTimezoneCorrectionRequest request)
	{
		var results = new List<ExifTimezoneCorrectionResult>();

		foreach ( var item in fileIndexItems )
		{
			var result = await CorrectTimezoneAsync(item, request);
			results.Add(result);
		}

		return results;
	}

	/// <summary>
	/// Validate timezone correction request
	/// </summary>
	public ExifTimezoneCorrectionResult ValidateCorrection(
		FileIndexItem fileIndexItem,
		ExifTimezoneCorrectionRequest request)
	{
		var result = new ExifTimezoneCorrectionResult
		{
			Success = false,
			OriginalDateTime = fileIndexItem.DateTime
		};

		// Validate timezones
		if ( string.IsNullOrWhiteSpace(request.RecordedTimezone) )
		{
			result.Error = "Recorded timezone is required";
			return result;
		}

		if ( string.IsNullOrWhiteSpace(request.CorrectTimezone) )
		{
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
			result.Error = $"Invalid recorded timezone: {request.RecordedTimezone}";
			return result;
		}

		try
		{
			_ = TimeZoneInfo.FindSystemTimeZoneById(request.CorrectTimezone);
		}
		catch ( Exception )
		{
			result.Error = $"Invalid correct timezone: {request.CorrectTimezone}";
			return result;
		}

		// Validate DateTime
		if ( fileIndexItem.DateTime.Year < 2 )
		{
			result.Error = "Image does not have a valid DateTime in EXIF";
			return result;
		}

		// Warn if timezones are the same
		if ( request.RecordedTimezone == request.CorrectTimezone )
		{
			result.Warning = "Recorded and correct timezones are the same - no correction needed";
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
			result.Warning = $"Correction will change the day from {fileIndexItem.DateTime:yyyy-MM-dd} to {correctedDateTime:yyyy-MM-dd}";
		}

		return result;
	}

	/// <summary>
	/// Calculate the timezone offset delta between recorded and correct timezones
	/// This method is DST-aware and calculates offsets based on the actual date
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

