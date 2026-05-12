using System.Threading.Tasks;

namespace starsky.foundation.import.Interfaces;

public interface IImportJsonCli
{
	/// <summary>
	///     Import a JSON file with the same format as the export JSON file
	///     ->
	///     --importindex-export-json
	///     --importindex-import-json
	/// </summary>
	/// <param name="args">The path to the JSON file</param>
	/// <returns>True if the import was successful, false otherwise</returns>
	Task<bool> ImportExportByArgs(string[] args);
}
