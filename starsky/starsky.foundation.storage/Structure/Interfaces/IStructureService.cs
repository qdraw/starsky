namespace starsky.foundation.storage.Structure.Interfaces;

public interface IStructureService
{
	string ParseSubfolders(StructureInputModel inputModel);

	string ParseSubfolders(int? getSubPathRelative, string fileNameBase = "",
		string extensionWithoutDot = "");

	string ParseFileName(StructureInputModel inputModel);
}
