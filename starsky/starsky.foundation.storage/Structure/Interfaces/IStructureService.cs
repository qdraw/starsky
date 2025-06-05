namespace starsky.foundation.storage.Structure.Interfaces;

public interface IStructureService
{
	string ParseSubfolders(StructureInputModel inputModel);
	string ParseFileName(StructureInputModel inputModel);
}
