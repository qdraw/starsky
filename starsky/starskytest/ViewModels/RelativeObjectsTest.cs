using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.ViewModels
{
	[TestClass]
	public class RelativeObjectsTest
	{
		[TestMethod]
		public void relativeObjects_args_1()
		{
			var relativeObjects = new RelativeObjects(true, new List<ColorClassParser.Color> {ColorClassParser.Color.Winner});
			var args = relativeObjects.Args;
			
			Assert.AreEqual(new KeyValuePair<string, string>("colorclass", "1"), args.FirstOrDefault());
		}
		
		[TestMethod]
		public void relativeObjects_args_12()
		{
			var relativeObjects = new RelativeObjects(true, new List<ColorClassParser.Color> 
				{ColorClassParser.Color.Winner, ColorClassParser.Color.WinnerAlt});
			var args = relativeObjects.Args;
			Assert.AreEqual(new KeyValuePair<string, string>("colorclass", "1,2"), args.FirstOrDefault());
		}
		
		[TestMethod]
		public void relativeObjects_args_112()
		{
			var relativeObjects = new RelativeObjects(true, new List<ColorClassParser.Color> {ColorClassParser.Color.Winner, 
				ColorClassParser.Color.Winner, ColorClassParser.Color.WinnerAlt});
			var args = relativeObjects.Args;
			Assert.AreEqual(new KeyValuePair<string, string>("colorclass", "1,1,2"), args.FirstOrDefault());
		}
	}
}
