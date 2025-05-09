using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starskytest.FakeMocks;

public class FakeSelectorStorage : ISelectorStorage
{
	private IStorage _storage = new FakeIStorage();

	public FakeSelectorStorage(IStorage? storage = null)
	{
		if ( storage != null )
		{
			_storage = storage;
		}
	}

	public IStorage Get(SelectorStorage.StorageServices storageServices)
	{
		return _storage;
	}

	public void SetStorage(IStorage storage)
	{
		_storage = storage;
	}
}
