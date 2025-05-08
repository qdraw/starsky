using System.Collections.Generic;

namespace starsky.foundation.writemeta.Models;

public class ExifToolWriteResultModel(string command)
{
	public string Command { get; set; } = command;
	public List<string> NewFileHashes { get; set; } = [];
	public List<(bool, bool, string?)> NewFileHashesStatuses { get; set; } = [];
}
