using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Services
{
	[TestClass]
	public class StructureServiceTest
	{
		[TestMethod]
		public void Test01()
		{
			var storage = new FakeIStorage();
			var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss*_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).GetSubPaths(new DateTime(2020, 01, 01, 01, 01, 01));
		}
	}
}
