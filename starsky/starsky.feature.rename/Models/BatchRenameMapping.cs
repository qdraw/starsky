using System.Collections.Generic;

namespace starsky.feature.rename.Models;

/// <summary>
///     Represents a single file rename mapping in a batch rename operation
/// </summary>
public class BatchRenameMapping
{
	/// <summary>
	///     Original file path (subPath style, e.g., /folder/filename.jpg)
	/// </summary>
	public string SourceFilePath { get; set; } = string.Empty;

	/// <summary>
	///     New file path (subPath style)
	/// </summary>
	public string TargetFilePath { get; set; } = string.Empty;

	/// <summary>
	///     List of related file paths that will be renamed together (e.g., XMP sidecars)
	/// </summary>
	public List<(string source, string target)> RelatedFilePaths { get; set; } = new();

	/// <summary>
	///     Sequence number assigned to handle duplicate formatted dates
	/// </summary>
	public int SequenceNumber { get; set; } = 0;

	/// <summary>
	///     Indicates if this mapping has any validation errors
	/// </summary>
	public bool HasError { get; set; }

	/// <summary>
	///     Error message if mapping validation failed
	/// </summary>
	public string? ErrorMessage { get; set; }
}

