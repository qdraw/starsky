using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using starsky.feature.rename.Models;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.rename.Services;

/// <summary>
///     Service for detecting and repairing datetime patterns in filenames
/// </summary>
public class FilenameDatetimeRepairService
{
	// Common datetime patterns in filenames
	private static readonly List<DateTimePattern> DateTimePatterns =
	[
		// YYYYMMDD_HHMMSS
		new()
		{
			Regex = new Regex(@"(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})"),
			Format = "yyyyMMdd_HHmmss",
			Description = "YYYYMMDD_HHMMSS"
		},
		// YYYY-MM-DD_HH-MM-SS
		new()
		{
			Regex = new Regex(@"(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})-(\d{2})"),
			Format = "yyyy-MM-dd_HH-mm-ss",
			Description = "YYYY-MM-DD_HH-MM-SS"
		},
		// YYYYMMDD_HHMM
		new()
		{
			Regex = new Regex(@"(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})"),
			Format = "yyyyMMdd_HHmm",
			Description = "YYYYMMDD_HHMM"
		},
		// YYYYMMDD
		new()
		{
			Regex = new Regex(@"(\d{4})(\d{2})(\d{2})"),
			Format = "yyyyMMdd",
			Description = "YYYYMMDD"
		}
	];

	private readonly IWebLogger _logger;
	private readonly IQuery _query;
	private readonly IStorage _storage;

	public FilenameDatetimeRepairService(IQuery query, IStorage storage, IWebLogger logger)
	{
		_query = query;
		_storage = storage;
		_logger = logger;
	}

	/// <summary>
	///     Preview filename datetime repair for a list of file paths
	/// </summary>
	public List<FilenameDatetimeRepairMapping> PreviewRepair(
		List<string> filePaths,
		IExifTimeCorrectionRequest correctionRequest,
		bool collections = true)
	{
		var results = new List<FilenameDatetimeRepairMapping>();

		foreach ( var filePath in filePaths )
		{
			var mapping = new FilenameDatetimeRepairMapping { SourceFilePath = filePath };

			try
			{
				// Get file item from database
				var detailView = _query.SingleItem(filePath, null, collections, false);
				if ( detailView?.FileIndexItem is not { Status: FileIndexItem.ExifStatus.Ok } )
				{
					mapping.HasError = true;
					mapping.ErrorMessage = "File not found or has invalid status";
					results.Add(mapping);
					continue;
				}

				var fileItem = detailView.FileIndexItem;

				// Detect datetime pattern in filename
				var detectedPattern = DetectDateTimePattern(fileItem.FileName!);
				if ( detectedPattern == null )
				{
					mapping.HasError = true;
					mapping.ErrorMessage = "No datetime pattern detected in filename";
					results.Add(mapping);
					continue;
				}

				mapping.DetectedPattern = detectedPattern.Description;

				// Extract datetime from filename
				var extractedDateTime = ExtractDateTime(fileItem.FileName!, detectedPattern);
				if ( extractedDateTime == null )
				{
					mapping.HasError = true;
					mapping.ErrorMessage = "Failed to extract datetime from filename";
					results.Add(mapping);
					continue;
				}

				mapping.OriginalDateTime = extractedDateTime.Value;

				// Calculate time offset using the internal method
				var delta =
					ExifTimezoneCorrectionService.CalculateTimezoneOffsetDelta(
						extractedDateTime.Value, correctionRequest);
				mapping.OffsetHours = delta.TotalHours;

				// Apply correction
				var correctedDateTime = extractedDateTime.Value.Add(delta);
				mapping.CorrectedDateTime = correctedDateTime;

				// Generate new filename with corrected datetime
				var newFileName = ReplaceDateTime(fileItem.FileName!, detectedPattern,
					correctedDateTime);
				mapping.TargetFilePath =
					PathHelper.AddSlash(fileItem.ParentDirectory!) + newFileName;

				// Check for day rollover
				if ( correctedDateTime.Day != extractedDateTime.Value.Day )
				{
					mapping.Warning =
						$"Correction will change the day from {extractedDateTime.Value:yyyy-MM-dd} to {correctedDateTime:yyyy-MM-dd}";
				}

				// Get related files (sidecars)
				if ( collections && fileItem.CollectionPaths.Count != 0 )
				{
					mapping.RelatedFilePaths = fileItem.CollectionPaths;
				}
			}
			catch ( Exception ex )
			{
				mapping.HasError = true;
				mapping.ErrorMessage = $"Exception: {ex.Message}";
				_logger.LogError(ex,
					$"[FilenameDatetimeRepair] Error processing {filePath}: {ex.Message}");
			}

			results.Add(mapping);
		}

		return results;
	}

	// /// <summary>
	// ///     Calculate timezone offset delta (replicates logic from ExifTimezoneCorrectionService)
	// /// </summary>
	// private static TimeSpan CalculateTimezoneOffsetDelta(DateTime dateTime,
	// 	IExifTimeCorrectionRequest request)
	// {
	// 	return request switch
	// 	{
	// 		ExifTimezoneBasedCorrectionRequest timezoneRequest => CalculateTimezoneDelta(
	// 			dateTime, timezoneRequest.RecordedTimezoneId,
	// 			timezoneRequest.CorrectTimezoneId),
	// 		ExifCustomOffsetCorrectionRequest customOffsetRequest => CalculateCustomOffsetDelta(
	// 			dateTime, customOffsetRequest),
	// 		_ => throw new ArgumentException("Invalid request type", nameof(request))
	// 	};
	// }
	//
	// /// <summary>
	// ///     Calculate timezone delta between recorded and correct timezones (DST-aware)
	// /// </summary>
	// private static TimeSpan CalculateTimezoneDelta(DateTime dateTime, string recordedTimezone,
	// 	string correctTimezone)
	// {
	// 	var recordedTz = TimeZoneInfo.FindSystemTimeZoneById(recordedTimezone);
	// 	var correctTz = TimeZoneInfo.FindSystemTimeZoneById(correctTimezone);
	// 	var recordedOffset = recordedTz.GetUtcOffset(dateTime);
	// 	var correctOffset = correctTz.GetUtcOffset(dateTime);
	// 	return correctOffset - recordedOffset;
	// }
	//
	// /// <summary>
	// ///     Calculate custom offset delta from request parameters
	// /// </summary>
	// private static TimeSpan CalculateCustomOffsetDelta(DateTime dateTime,
	// 	ExifCustomOffsetCorrectionRequest request)
	// {
	// 	var adjustedDateTime = dateTime;
	// 	if ( request.Year != 0 )
	// 	{
	// 		adjustedDateTime = adjustedDateTime.AddYears(request.Year.GetValueOrDefault());
	// 	}
	//
	// 	if ( request.Month != 0 )
	// 	{
	// 		adjustedDateTime = adjustedDateTime.AddMonths(request.Month.GetValueOrDefault());
	// 	}
	//
	// 	if ( request.Day != 0 )
	// 	{
	// 		adjustedDateTime = adjustedDateTime.AddDays(request.Day.GetValueOrDefault());
	// 	}
	//
	// 	var delta = TimeSpan.Zero;
	// 	if ( request.Hour != 0 )
	// 	{
	// 		delta = delta.Add(TimeSpan.FromHours(request.Hour.GetValueOrDefault()));
	// 	}
	//
	// 	if ( request.Minute != 0 )
	// 	{
	// 		delta = delta.Add(TimeSpan.FromMinutes(request.Minute.GetValueOrDefault()));
	// 	}
	//
	// 	if ( request.Second != 0 )
	// 	{
	// 		delta = delta.Add(TimeSpan.FromSeconds(request.Second.GetValueOrDefault()));
	// 	}
	//
	// 	var totalDelta = adjustedDateTime - dateTime + delta;
	// 	return totalDelta;
	// }

	/// <summary>
	///     Execute filename datetime repair based on mappings
	/// </summary>
	public async Task<List<FileIndexItem>> ExecuteRepairAsync(
		List<FilenameDatetimeRepairMapping> mappings,
		bool collections = true)
	{
		var results = new List<FileIndexItem>();

		// Filter out error mappings
		var validMappings = mappings.Where(m => !m.HasError).ToList();

		foreach ( var mapping in validMappings )
		{
			try
			{
				var detailView =
					_query.SingleItem(mapping.SourceFilePath, null, collections, false);
				if ( detailView?.FileIndexItem == null )
				{
					_logger.LogError(
						$"[FilenameDatetimeRepair] File not found: {mapping.SourceFilePath}");
					continue;
				}

				// Check if source and target are the same
				if ( mapping.SourceFilePath == mapping.TargetFilePath )
				{
					_logger.LogInformation(
						$"[FilenameDatetimeRepair] Skipping, source and target are the same: {mapping.SourceFilePath}");
					results.Add(detailView.FileIndexItem);
					continue;
				}

				// Rename file using SubPath storage (no FullPath conversion needed)
				_storage.FileMove(mapping.SourceFilePath, mapping.TargetFilePath);

				// Update database
				var fileItem = detailView.FileIndexItem;
				fileItem.FileName = FilenamesHelper.GetFileName(mapping.TargetFilePath);
				fileItem.FilePath = mapping.TargetFilePath;

				await _query.UpdateItemAsync(fileItem);

				// Rename related files (sidecars)
				if ( collections && mapping.RelatedFilePaths.Count != 0 )
				{
					foreach ( var relatedPath in mapping.RelatedFilePaths )
					{
						var relatedTargetPath = GetRelatedTargetPath(relatedPath, mapping);

						if ( _storage.ExistFile(relatedPath) )
						{
							_storage.FileMove(relatedPath, relatedTargetPath);
						}
					}
				}

				_logger.LogInformation(
					$"[FilenameDatetimeRepair] Renamed: {mapping.SourceFilePath} → {mapping.TargetFilePath}");
				results.Add(fileItem);
			}
			catch ( Exception ex )
			{
				_logger.LogError(ex,
					$"[FilenameDatetimeRepair] Failed to rename {mapping.SourceFilePath}: {ex.Message}");
			}
		}

		return results;
	}

	/// <summary>
	///     Detect datetime pattern in filename
	/// </summary>
	private static DateTimePattern? DetectDateTimePattern(string fileName)
	{
		return DateTimePatterns.FirstOrDefault(pattern => pattern.Regex.IsMatch(fileName));
	}

	/// <summary>
	///     Extract datetime from filename using detected pattern
	/// </summary>
	private static DateTime? ExtractDateTime(string fileName, DateTimePattern pattern)
	{
		var match = pattern.Regex.Match(fileName);
		if ( !match.Success )
		{
			return null;
		}

		var dateTimeString = match.Value;
		if ( DateTime.TryParseExact(dateTimeString, pattern.Format,
			    CultureInfo.InvariantCulture,
			    DateTimeStyles.None, out var dateTime) )
		{
			return dateTime;
		}

		return null;
	}

	/// <summary>
	///     Replace datetime in filename with corrected datetime
	/// </summary>
	private static string ReplaceDateTime(string fileName, DateTimePattern pattern,
		DateTime correctedDateTime)
	{
		var match = pattern.Regex.Match(fileName);
		if ( !match.Success )
		{
			return fileName;
		}

		var originalDateTimeString = match.Value;
		var correctedDateTimeString = correctedDateTime.ToString(pattern.Format);
		return fileName.Replace(originalDateTimeString, correctedDateTimeString);
	}

	/// <summary>
	///     Get target path for related file (sidecar)
	/// </summary>
	private static string GetRelatedTargetPath(string relatedPath,
		FilenameDatetimeRepairMapping mapping)
	{
		var relatedFileName = FilenamesHelper.GetFileName(relatedPath);
		var sourceFileNameWithoutExtension =
			FilenamesHelper.GetFileNameWithoutExtension(
				FilenamesHelper.GetFileName(mapping.SourceFilePath));
		var targetFileNameWithoutExtension =
			FilenamesHelper.GetFileNameWithoutExtension(
				FilenamesHelper.GetFileName(mapping.TargetFilePath));

		var newRelatedFileName = relatedFileName.Replace(sourceFileNameWithoutExtension,
			targetFileNameWithoutExtension);
		return PathHelper.AddSlash(FilenamesHelper.GetParentPath(relatedPath)) +
		       newRelatedFileName;
	}

	/// <summary>
	///     Internal class for datetime pattern definition
	/// </summary>
	private sealed class DateTimePattern
	{
		public required Regex Regex { get; init; }
		public required string Format { get; init; }
		public required string Description { get; init; }
	}
}
