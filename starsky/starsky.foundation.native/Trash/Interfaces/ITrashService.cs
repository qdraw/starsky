namespace starsky.foundation.native.Trash.Interfaces;

public interface ITrashService
{
	bool DetectToUseSystemTrash();
	bool? Trash(string fullPath);
	bool? Trash(List<string> fullPaths);
}
