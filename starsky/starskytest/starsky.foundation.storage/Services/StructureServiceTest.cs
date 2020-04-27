using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.storage.Services
{
	[TestClass]
	public class StructureServiceTest
	{
		[TestMethod]
		public void GetSubPaths_GetExistingFolder()
		{
			var storage = new FakeIStorage(
				new List<string>{"/2020","/2020/01","/2020/01/2020_01_01 test"},
				new List<string>());
			
			var structure = "/yyyy/MM/yyyy_MM_dd*/yyyyMMdd_HHmmss_{filenamebase}.ext";
			
			var result = new StructureService(storage,structure).GetSubPaths(
				new DateTime(2020, 01, 01, 01, 01, 01));
		}
	}
}
