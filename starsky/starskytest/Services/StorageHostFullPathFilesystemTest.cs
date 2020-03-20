using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Storage;
using starskycore.Services;

namespace starskytest.Services
{
	[TestClass]
	public class StorageHostFullPathFilesystemTest
	{
		[TestMethod]
		public void Files_GetFilesRecrusiveTest()
		{            
			var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;

			var content = new StorageHostFullPathFilesystem().GetAllFilesInDirectoryRecursive(path);

			Console.WriteLine("count => "+ content.Count());

			// Gives a list of the content in the temp folder.
			Assert.AreEqual(true, content.Any());            

		}
	}
}
