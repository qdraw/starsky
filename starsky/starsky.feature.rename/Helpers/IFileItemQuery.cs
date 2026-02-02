namespace starsky.feature.rename.Helpers;

public interface IFileItemQuery
{
	/// <summary>
	///     Source file path
	/// </summary>
	public string SourceFilePath { get; set; }

	/// <summary>
	///     Indicates if there was an error
	/// </summary>
	public bool HasError { get; set; }

	/// <summary>
	///     Error message if HasError is true
	/// </summary>
	public string? ErrorMessage { get; set; }
}
