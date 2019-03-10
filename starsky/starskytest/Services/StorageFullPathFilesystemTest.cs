using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Services;

namespace starskytest.Services
{
	[TestClass]
	public class StorageFullPathFilesystemTest
	{
		[TestMethod]
		public void Files_GetFilesRecrusiveTest()
		{            
			var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;

			var content = new StorageFullPathFilesystem().GetAllFilesInDirectoryRecursive(path);

			Console.WriteLine("count => "+ content.Count());

			// Gives a list of the content in the temp folder.
			Assert.AreEqual(true, content.Any());            

		}
	}
}
