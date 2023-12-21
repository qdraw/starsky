using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.georealtime.Models.GeoJson;

namespace starskytest.starsky.foundation.georealtime.Models;

[TestClass]
public class GeometryModelTest
{
	
	[TestMethod]
	public void NoItemsInList()
	{
		var model = new GeometryModel { Coordinates = new List<List<double>>() };
		Assert.AreEqual(0,model.Coordinates.Count);
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void InvalidList()
	{
		var geometryModel = new GeometryModel { Coordinates = new List<List<double>>{new List<double>()} };
		
		Assert.IsNull(geometryModel);
	}
	
	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void InvalidList_OneItem()
	{
		var geometryModel = new GeometryModel { Coordinates = new List<List<double>>{new List<double>{0}} };
		
		Assert.IsNull(geometryModel);
	}
		
	[TestMethod]
	public void InvalidList_TwoItems()
	{
		var geometryModel = new GeometryModel { Coordinates = new List<List<double>>{new List<double>{0,1}} };
		
		Assert.AreEqual(0,geometryModel.Coordinates[0][0]);
		Assert.AreEqual(1,geometryModel.Coordinates[0][1]);
	}
	
	[TestMethod]
	public void InvalidList_ThreeItems()
	{
		var geometryModel = new GeometryModel { Coordinates = new List<List<double>>{new List<double>{0,1,2}} };
		
		Assert.AreEqual(0,geometryModel.Coordinates[0][0]);
		Assert.AreEqual(1,geometryModel.Coordinates[0][1]);
		Assert.AreEqual(2,geometryModel.Coordinates[0][2]);
	}
}
