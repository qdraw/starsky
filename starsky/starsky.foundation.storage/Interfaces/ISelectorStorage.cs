using starsky.foundation.storage.Storage;

namespace starsky.foundation.storage.Interfaces
{
	/// <summary>
	/// ISelectionStorage
	/// </summary>
	public interface ISelectorStorage
	{
		IStorage Get(SelectorStorage.StorageServices storageServices);
	}
}
