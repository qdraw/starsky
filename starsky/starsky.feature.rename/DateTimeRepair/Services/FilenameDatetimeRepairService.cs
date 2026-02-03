using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.rename.DateTimeRepair.Helpers;
using starsky.feature.rename.DateTimeRepair.Models;
using starsky.feature.rename.Helpers;
using starsky.feature.rename.Models;
using starsky.feature.rename.RelatedFilePaths;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.rename.DateTimeRepair.Services;

/// <summary>
///     Service for detecting and repairing datetime patterns in filenames
/// </summary>
public class FilenameDatetimeRepairService(
	IQuery query,
	IStorage storage,
	IWebLogger logger,
	AppSettings appSettings)
{
	/// <summary>
	///     Preview filename datetime repair for a list of file paths
	/// </summary>
	public List<FilenameDatetimeRepairMapping> PreviewRepair(
		List<string> filePaths,
		IExifTimeCorrectionRequest correctionRequest,
		bool collections = true)
	{
		var mappings = new List<FilenameDatetimeRepairMapping>();
		var fileItemsQuery = new FileItemsQueryHelpers(query, logger);

		var fileItems = fileItemsQuery.FileItemsQuery(
			filePaths, collections, mappings);
		var validMappings =
			GenerateValidMappings(correctionRequest,
				fileItems, collections, mappings);

		mappings.AddRange(validMappings);
		return mappings;
	}

	private List<FilenameDatetimeRepairMapping> GenerateValidMappings(
		IExifTimeCorrectionRequest correctionRequest,
		Dictionary<string, FileIndexItem> fileItems,
		bool collections, List<FilenameDatetimeRepairMapping> mappings)
	{
		var validMappings = new List<FilenameDatetimeRepairMapping>();
		foreach ( var (key, fileItem) in fileItems )
		{
			var mapping = new FilenameDatetimeRepairMapping { SourceFilePath = key };

			if ( new StatusCodesHelper(appSettings).IsReadOnlyStatus(fileItem) !=
			     FileIndexItem.ExifStatus.Default )
			{
				mappings.Add(new FilenameDatetimeRepairMapping
				{
					SourceFilePath = key, HasError = true, ErrorMessage = "Read-only location"
				});
				continue;
			}

			if ( fileItem.IsDirectory == true )
			{
				mappings.Add(new FilenameDatetimeRepairMapping
				{
					SourceFilePath = key, HasError = true, ErrorMessage = "Is a directory"
				});
				continue;
			}

			// Detect datetime pattern in filename
			var detectedPattern = DetectDateTimePattern(fileItem.FileName!);
			if ( detectedPattern == null )
			{
				mapping.HasError = true;
				mapping.ErrorMessage = "No datetime pattern detected in filename";
				mappings.Add(mapping);
				continue;
			}

			mapping.DetectedPatternDescription = detectedPattern.Description;

			// Extract datetime from filename
			var extractedDateTime = ExtractDateTime(fileItem.FileName!, detectedPattern);
			if ( extractedDateTime == null )
			{
				mapping.HasError = true;
				mapping.ErrorMessage = "Failed to extract datetime from filename";
				mappings.Add(mapping);
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

			if ( collections )
			{
				mapping.RelatedFilePaths =
					new ReleatedFilePaths(storage).GetRelatedFilePaths(key, mapping.TargetFilePath);
			}

			validMappings.Add(mapping);
		}

		return validMappings;
	}

	public async Task<List<FileIndexItem>> ExecuteRepairAsync(
		List<string> filePaths,
		IExifTimeCorrectionRequest correctionRequest,
		bool collections = true)
	{
		var mappings = PreviewRepair(filePaths, correctionRequest, collections);
		return await ExecuteRepairAsync(mappings, collections);
	}

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
				mapping.FileIndexItem ??= query.SingleItem(mapping.SourceFilePath,
					null, collections)?.FileIndexItem;
				if ( mapping.FileIndexItem == null )
				{
					logger.LogError(
						$"[FilenameDatetimeRepair] File not found: " +
						$"{mapping.SourceFilePath}");
					continue;
				}

				// Check if source and target are the same
				if ( mapping.SourceFilePath == mapping.TargetFilePath )
				{
					logger.LogInformation(
						$"[FilenameDatetimeRepair] Skipping, " +
						$"source and target are the same: {mapping.SourceFilePath}");
					results.Add(mapping.FileIndexItem);
					continue;
				}

				// Clone the original FileIndexItem and set status to Deleted
				var deletedItem = mapping.FileIndexItem.Clone();
				deletedItem.Status = FileIndexItem.ExifStatus.Deleted;
				results.Add(deletedItem);

				// Rename file using SubPath storage 
				storage.FileMove(mapping.SourceFilePath, mapping.TargetFilePath);

				// Update database
				mapping.FileIndexItem.FileName =
					FilenamesHelper.GetFileName(mapping.TargetFilePath);
				mapping.FileIndexItem.FilePath = mapping.TargetFilePath;
				mapping.FileIndexItem.Status = FileIndexItem.ExifStatus.Ok;

				await query.UpdateItemAsync(mapping.FileIndexItem);

				// Rename related files (sidecars)
				if ( collections && mapping.RelatedFilePaths.Count != 0 )
				{
					foreach ( var (source, target) in mapping.RelatedFilePaths )
					{
						if ( storage.ExistFile(source) )
						{
							storage.FileMove(source, target);
						}
					}
				}

				// Reset Cache for the item that is renamed
				query.ResetItemByHash(mapping.FileIndexItem!.FileHash!);

				logger.LogInformation(
					$"[FilenameDatetimeRepair] Renamed: " +
					$"{mapping.SourceFilePath} → {mapping.TargetFilePath}");
				results.Add(mapping.FileIndexItem);
			}
			catch ( Exception ex )
			{
				logger.LogError(ex,
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
		return DateTimePatterns.DateTimePatternList.FirstOrDefault(pattern =>
			pattern.Regex.IsMatch(fileName));
	}

	/// <summary>
	///     Extract datetime from filename using detected pattern
	/// </summary>
	internal static DateTime? ExtractDateTime(string fileName, DateTimePattern pattern)
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
	internal static string ReplaceDateTime(string fileName, DateTimePattern pattern,
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
}
