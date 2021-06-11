using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.MetaThumbnail;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.readmeta.Services
{
	[TestClass]
	public class ReadMetaThumbnailTest
	{
		private readonly FakeIStorage _iStorageFake;
		private readonly string _exampleHash;

		public ReadMetaThumbnailTest()
		{
			_iStorageFake = new FakeIStorage(
				new List<string>{"/"},
				new List<string>{"/test.jpg","/test2.jpg"},
				new List<byte[]>{CreateAnImage.Bytes,CreateAnImage.Bytes}
				);
			
			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/test.jpg").Key;
		}
		
		[TestMethod]
		public void Test()
		{
			
			new ReadMetaThumbnail(new FakeSelectorStorage(_iStorageFake), new FakeIWebLogger())
				.ReadExifFromFile2("/test.jpg");
			Console.WriteLine();
		}


				
		[TestMethod]
		public async Task Test2()
		{
			
			await new ReadMetaThumbnail(new FakeSelectorStorage(new StorageHostFullPathFilesystem()), new FakeIWebLogger())
				.ReadExifFromFile2("/data/scripts/__starsky/2021_02_07 performance sneeuw/20210207_112755_DSC04053.jpg");
			Console.WriteLine();
		}
		
	}
}
