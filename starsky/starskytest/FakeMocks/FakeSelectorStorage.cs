using starskycore.Interfaces;
using starskycore.Services;

namespace starskytest.FakeMocks
{
	public class FakeSelectorStorage : ISelectorStorage
	{
		public IStorage Get(SelectorStorage.StorageServices storageServices)
		{
			return new FakeIStorage();
		}
	}
}
