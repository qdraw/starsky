using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starskytest.FakeMocks
{
	public class FakeSelectorStorage : ISelectorStorage
	{
		private readonly IStorage _storage;
		
		public FakeSelectorStorage(IStorage? storage = null)
		{
			if(storage != null) _storage = storage;
			if(storage == null) _storage = new FakeIStorage();
		}
		public IStorage Get(SelectorStorage.StorageServices storageServices)
		{
			return _storage;
		}
	}
}
