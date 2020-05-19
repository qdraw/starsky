using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Storage;

namespace starskytest.starsky.foundation.storage.ArchiveFormats
{
	[TestClass]
	public class TarBalTest
	{
		private readonly StorageHostFullPathFilesystem _hostStorageProvider;

		public TarBalTest()
		{
			_hostStorageProvider = new StorageHostFullPathFilesystem();
		}
		[TestMethod]
		public void Test()
		{
			
			// _hostStorageProvider.WriteStream() 
		}
	}
}
