using System.Collections.Generic;

namespace starsky.foundation.writemeta.Models;

public class ExifToolCmdHelperWriteResultModel(string command)
{
	public string Command { get; set; } = command;
	public List<string> NewFileHashes { get; set; } = [];
	public List<ExifToolWriteTagsAndRenameThumbnailModel> Rename { get; set; } = [];
	
}
