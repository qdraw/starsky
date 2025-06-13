namespace starsky.foundation.storage.Structure.Interfaces;

public interface IStructureService
{
	string ParseSubfolders(int? getSubPathRelative, string fileNameBase = "",
		string extensionWithoutDot = "", string source = "");

	string ParseSubfolders(StructureInputModel inputModel);

	string ParseFileName(StructureInputModel inputModel);
}
