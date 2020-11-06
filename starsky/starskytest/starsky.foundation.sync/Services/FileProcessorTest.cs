using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.Helpers;
using starsky.foundation.sync.Services;
using starskytest.FakeCreateAn;

namespace starskytest.starsky.foundation.sync.Services
{
	[TestClass]
	public class FileProcessorTest
	{
		private List<string> IsExecuted { get; set; } = new List<string>();

		private void TestExecuted(string filePath)
		{
			IsExecuted.Add(filePath);
		}
		
		[TestMethod]
		public void CheckInput()
		{
			var fileProcessor = new FileProcessor(new AutoResetEventAsync(), TestExecuted );
			
			fileProcessor.QueueInput(new CreateAnImage().BasePath);
			fileProcessor.QueueInput("/test2");

			fileProcessor.Work();

			Assert.AreEqual(2, IsExecuted.Count);
			Assert.AreEqual(new CreateAnImage().BasePath, IsExecuted[0]);
			Assert.AreEqual("/test2", IsExecuted[1]);
			
			IsExecuted = new List<string>();
		}
	}
}
