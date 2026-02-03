using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using starsky.feature.rename.Helpers;
using starsky.feature.rename.Models;
using starsky.feature.rename.RelatedFilePaths;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;

namespace starsky.feature.rename.Services;

public class BatchRenameService(
	IQuery query,
	IStorage iStorage,
	IWebLogger logger,
	AppSettings appSettings)
{
	/// <summary>
	///     Preview batch rename operation without modifying files
	/// </summary>
	/// <param name="filePaths">List of source file paths to rename</param>
	/// <param name="tokenPattern">
	///     Rename pattern with tokens
	///     (e.g., {yyyy}{MM}{dd}_{filenamebase}{seqn}.ext)
	/// </param>
	/// <param name="collections">Include related sidecar files</param>
	/// <returns>List of batch rename mappings with preview of new names</returns>
	public List<BatchRenameMapping> PreviewBatchRename(
		List<string> filePaths, string tokenPattern, bool collections = true)
	{
		if ( filePaths.Count == 0 )
		{
			return [];
		}

		var pattern = new RenameTokenPattern(tokenPattern);
		if ( !pattern.IsValid )
		{
			return filePaths.Select(fp => new BatchRenameMapping
			{
				SourceFilePath = fp,
				HasError = true,
				ErrorMessage = $"Invalid pattern: {string.Join(", ", pattern.Errors)}"
			}).ToList();
		}

		// Get all file items from database
		var mappings = new List<BatchRenameMapping>();
		var fileItemsQuery = new FileItemsQueryHelpers(query, logger);
		var fileItems =
			fileItemsQuery.FileItemsQuery(filePaths, collections, mappings);

		// Generate mappings for valid files
		var validMappings = GenerateValidMappings(
			fileItems, collections, pattern, mappings);

		// Assign sequence numbers for duplicate target names
		AssignSequenceNumbers(validMappings, pattern, fileItems);

		mappings.AddRange(validMappings);
		return mappings;
	}

	private List<BatchRenameMapping> GenerateValidMappings(
		Dictionary<string, FileIndexItem> fileItems,
		bool collections,
		RenameTokenPattern pattern,
		List<BatchRenameMapping> mappings)
	{
		var validMappings = new List<BatchRenameMapping>();
		foreach ( var (key, fileItem) in fileItems )
		{
			if ( fileItem.IsDirectory == true )
			{
				mappings.Add(new BatchRenameMapping
				{
					SourceFilePath = key, HasError = true, ErrorMessage = "Is a directory"
				});
				continue;
			}

			if ( new StatusCodesHelper(appSettings).IsReadOnlyStatus(fileItem) !=
			     FileIndexItem.ExifStatus.Default )
			{
				mappings.Add(new BatchRenameMapping
				{
					SourceFilePath = key, HasError = true, ErrorMessage = "Read-only location"
				});
				continue;
			}

			try
			{
				var parentPath = fileItem.ParentDirectory;
				var newFileName = pattern.GenerateFileName(fileItem);
				var newFilePath = $"{parentPath}/{newFileName}";
				if ( parentPath == "/" )
				{
					newFilePath = $"/{newFileName}";
				}

				var mapping = new BatchRenameMapping
				{
					SourceFilePath = key,
					// where to
					TargetFilePath = newFilePath,
					// SequenceNumber follows in a later step
					SequenceNumber = 0
				};

				// Add related files (sidecars)
				if ( collections )
				{
					mapping.RelatedFilePaths =
						new ReleatedFilePaths(iStorage).GetRelatedFilePaths(key, newFilePath);
				}

				validMappings.Add(mapping);
			}
			catch ( Exception ex )
			{
				mappings.Add(new BatchRenameMapping
				{
					SourceFilePath = key,
					HasError = true,
					ErrorMessage = $"Failed to generate filename: {ex.Message}"
				});
			}
		}

		return validMappings;
	}

	/// <summary>
	///     Execute batch rename operation
	/// </summary>
	/// <param name="mappings">List of rename mappings from preview</param>
	/// <param name="collections">Include related sidecar files</param>
	/// <returns>List of updated file index items with status</returns>
	public async Task<List<FileIndexItem>> ExecuteBatchRenameAsync(
		List<BatchRenameMapping> mappings, bool collections = true)
	{
		if ( mappings.Count == 0 )
		{
			return [];
		}

		var results = new List<FileIndexItem>();

		// Filter out error mappings
		var validMappings = mappings.Where(m => !m.HasError).ToList();

		foreach ( var mapping in validMappings )
		{
			try
			{
				var detailView = query.SingleItem(mapping.SourceFilePath, null, collections, false);
				if ( detailView?.FileIndexItem == null )
				{
					results.Add(new FileIndexItem(mapping.SourceFilePath)
					{
						Status = FileIndexItem.ExifStatus.NotFoundNotInIndex
					});
					continue;
				}

				// Use existing FromFileToDeleted logic to perform rename
				var fileIndexItems = new List<FileIndexItem>();
				await new RenameService(query, iStorage, logger).RenameFromFileToDeleted(
					mapping.SourceFilePath, mapping.TargetFilePath,
					results, fileIndexItems, detailView);

				// Reset Cache for the item that is renamed
				query.ResetItemByHash(detailView.FileIndexItem!.FileHash!);
			}
			catch ( Exception )
			{
				results.Add(new FileIndexItem(mapping.SourceFilePath)
				{
					Status = FileIndexItem.ExifStatus.OperationNotSupported
				});
			}
		}

		return results;
	}

	/// <summary>
	///     Assign sequence numbers to files with identical target names
	/// </summary>
	private void AssignSequenceNumbers(
		List<BatchRenameMapping> mappings,
		RenameTokenPattern pattern,
		Dictionary<string, FileIndexItem> fileItems)
	{
		// Group by original filename base and datetime
		var grouped = mappings
			.GroupBy(m =>
			{
				var fileItem = fileItems[m.SourceFilePath];
				return new
				{
					OriginalBase =
						FilenamesHelper.GetFileNameWithoutExtension(fileItem.FileName!),
					fileItem.DateTime
				};
			})
			.ToList();

		var sequence = 0;
		foreach ( var group in grouped.OrderBy(g => g.Key.OriginalBase)
			         .ThenBy(g => g.Key.DateTime) )
		{
			foreach ( var mapping in group )
			{
				var fileItem = fileItems[mapping.SourceFilePath];
				var newFileName = pattern.GenerateFileName(fileItem, sequence);
				var newFilePath = $"{fileItem.ParentDirectory}/{newFileName}";
				if ( fileItem.ParentDirectory == "/" )
				{
					newFilePath = $"/{newFileName}";
				}

				mapping.TargetFilePath = newFilePath;
				mapping.SequenceNumber = sequence;
				if ( mapping.RelatedFilePaths.Count > 0 )
				{
					mapping.RelatedFilePaths =
						new ReleatedFilePaths(iStorage).GetRelatedFilePaths(mapping.SourceFilePath,
							newFilePath);
				}
			}

			sequence++;
		}
	}
}
