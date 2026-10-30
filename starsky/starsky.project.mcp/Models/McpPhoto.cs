using starsky.foundation.database.Models;

namespace starsky.project.mcp.Models;

/// <summary>
///     Compact view of a file or folder, so tool results stay small for the agent's context
/// </summary>
public sealed record McpPhoto(
	string? Path,
	string? Name,
	bool IsDirectory,
	string? Tags,
	string? Description,
	string? Title,
	string ColorClass,
	DateTime DateTime,
	string Status,
	double? Latitude,
	double? Longitude,
	string? LocationCity,
	string? LocationCountry)
{
	public static McpPhoto FromItem(FileIndexItem item)
	{
		var hasLocation = item.Latitude != 0 || item.Longitude != 0;
		return new McpPhoto(item.FilePath, item.FileName, item.IsDirectory == true,
			item.Tags, item.Description, item.Title, item.ColorClass.ToString(),
			item.DateTime, item.Status.ToString(),
			hasLocation ? item.Latitude : null, hasLocation ? item.Longitude : null,
			item.LocationCity, item.LocationCountry);
	}

	public static List<McpPhoto> FromItems(IEnumerable<FileIndexItem> items, int max = int.MaxValue)
	{
		var result = new List<McpPhoto>();
		foreach ( var item in items )
		{
			if ( result.Count >= max )
			{
				break;
			}

			result.Add(FromItem(item));
		}

		return result;
	}
}
