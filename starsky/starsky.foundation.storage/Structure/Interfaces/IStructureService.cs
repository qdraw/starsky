using System;
using System.Collections.Generic;
using starsky.foundation.storage.Models;

namespace starsky.foundation.storage.Structure.Interfaces;

public interface IStructureService
{
	string ParseSubfolders(int? getSubPathRelative, string fileNameBase = "",
		string extensionWithoutDot = "", string source = "");

	string ParseSubfolders(StructureInputModel inputModel);

	string ParseFileName(StructureInputModel inputModel);

	List<List<StructureRange>> ParseStructure(string structure,
		DateTime dateTime,
		string fileNameBase = "", string extensionWithoutDot = "");
}
