using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Services;
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
			_iStorageFake = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg","/test2.jpg"},
				new List<byte[]>{CreateAnImage.Bytes,CreateAnImage.Bytes});
			
			_exampleHash = new FileHash(_iStorageFake).GetHashCode("/test.jpg").Key;
		}
		
		[TestMethod]
		public void Test()
		{
			var storage = new FakeIStorage();
			
			new ReadMetaThumbnail(new FakeSelectorStorage(storage))
				.ReadExifFromFile("/test.jpg");
			Console.WriteLine();
		}
	}
}
