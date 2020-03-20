using starsky.foundation.storage.Storage;

namespace starsky.foundation.storage.Interfaces
{
	public interface ISelectorStorage
	{
		IStorage Get(SelectorStorage.StorageServices storageServices);
	}
}
