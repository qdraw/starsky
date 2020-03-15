using starsky.foundation.storage.Storage;
using starskycore.Interfaces;

namespace starsky.foundation.storage.Interfaces
{
	public interface ISelectorStorage
	{
		IStorage Get(SelectorStorage.StorageServices storageServices);
	}
}
