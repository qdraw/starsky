using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.metathumbnail.Services;
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
		
		// [TestMethod]
		// public void Test()
		// {
		// 	
		// 	new ReadMetaThumbnail(new FakeSelectorStorage(_iStorageFake), new FakeIWebLogger())
		// 		.AddMetaThumbnail("/test.jpg","/temp/test.jpg");
		// 	Console.WriteLine();
		// }
		//
		//
		// 		
		// [TestMethod]
		// public async Task Test2()
		// {
		// 	
		// 	await new ReadMetaThumbnail(new FakeSelectorStorage(new StorageHostFullPathFilesystem()), new FakeIWebLogger())
		// 		.AddMetaThumbnail("/data/scripts/__starsky/2021_02_07 performance sneeuw/20210207_112755_DSC04053.jpg","/temp/test.jpg");
		// 	Console.WriteLine();
		// }
		//
		// [TestMethod]
		// public async Task Test3()
		// {
		// 	
		// 	await new ReadMetaThumbnail(new FakeSelectorStorage(new StorageHostFullPathFilesystem()), new FakeIWebLogger())
		// 		.AddMetaThumbnail("/data/scripts/__starsky/00_demo/20210530_140259_DSC03138.jpg","/temp/test.jpg");
		// 	Console.WriteLine();
		// }
		//
		// [TestMethod]
		// public async Task Test4()
		// {
		// 	
		// 	await new MetaExifThumbnailService(new FakeSelectorStorage(new StorageHostFullPathFilesystem()), new FakeIWebLogger())
		// 		.AddMetaThumbnail("/data/scripts/__starsky/00_demo/20201005_155330_DSC05634.jpg","/temp/test");
		// 	Console.WriteLine();
		// }
		//
		
	}
}
