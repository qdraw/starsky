using starskycore.Services;

namespace starskycore.Interfaces
{
	public interface ISelectorStorage
	{
		IStorage Get(SelectorStorage.StorageServices storageServices);
	}
}
