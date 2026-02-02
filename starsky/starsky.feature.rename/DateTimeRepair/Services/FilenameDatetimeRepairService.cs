using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.rename.DateTimeRepair.Helpers;
using starsky.feature.rename.DateTimeRepair.Models;
using starsky.feature.rename.Models;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.metaupdate.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.rename.DateTimeRepair.Services;

/// <summary>
///     Service for detecting and repairing datetime patterns in filenames
/// </summary>
public class FilenameDatetimeRepairService(IQuery query, IStorage storage, IWebLogger logger)
{
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
				var detailView = query.SingleItem(filePath,
					null, collections, false);
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
				logger.LogError(ex,
					$"[FilenameDatetimeRepair] Error processing {filePath}: {ex.Message}");
			}

			results.Add(mapping);
		}

		return results;
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
				var detailView =
					query.SingleItem(mapping.SourceFilePath, null, collections, false);
				if ( detailView?.FileIndexItem == null )
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
					results.Add(detailView.FileIndexItem);
					continue;
				}

				// Rename file using SubPath storage (no FullPath conversion needed)
				storage.FileMove(mapping.SourceFilePath, mapping.TargetFilePath);

				// Update database
				var fileItem = detailView.FileIndexItem;
				fileItem.FileName = FilenamesHelper.GetFileName(mapping.TargetFilePath);
				fileItem.FilePath = mapping.TargetFilePath;

				await query.UpdateItemAsync(fileItem);

				// Rename related files (sidecars)
				if ( collections && mapping.RelatedFilePaths.Count != 0 )
				{
					foreach ( var relatedPath in mapping.RelatedFilePaths )
					{
						var relatedTargetPath = GetRelatedTargetPath(relatedPath, mapping);

						if ( storage.ExistFile(relatedPath) )
						{
							storage.FileMove(relatedPath, relatedTargetPath);
						}
					}
				}

				logger.LogInformation(
					$"[FilenameDatetimeRepair] Renamed: " +
					$"{mapping.SourceFilePath} → {mapping.TargetFilePath}");
				results.Add(fileItem);
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
}
