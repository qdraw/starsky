using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.JsonConverters;

[TestClass]
public sealed class FileIndexItemWithIdJsonConverterFactoryTest
{
	[TestMethod]
	public void CanConvert_ReturnsExpected()
	{
		var factory = new FileIndexItemWithIdJsonConverterFactory();

		Assert.IsTrue(factory.CanConvert(typeof(FileIndexItem)));
		Assert.IsTrue(factory.CanConvert(typeof(FileIndexItem[])));
		Assert.IsTrue(factory.CanConvert(typeof(List<FileIndexItem>)));
		Assert.IsTrue(factory.CanConvert(typeof(IList<FileIndexItem>)));
		Assert.IsTrue(factory.CanConvert(typeof(IEnumerable<FileIndexItem>)));

		Assert.IsFalse(factory.CanConvert(typeof(string)));
		Assert.IsFalse(factory.CanConvert(typeof(List<string>)));
	}

	[TestMethod]
	public void CreateConverter_Unsupported_Throws()
	{
		var factory = new FileIndexItemWithIdJsonConverterFactory();
		var options = new JsonSerializerOptions();
		Assert.ThrowsExactly<NotSupportedException>(() =>
			factory.CreateConverter(typeof(string), options));
	}

	[TestMethod]
	public void InnerConverters_ReadWrite_Behavior()
	{
		var factory = new FileIndexItemWithIdJsonConverterFactory();
		var options = new JsonSerializerOptions();

		// List converter
		var listConv =
			( JsonConverter<List<FileIndexItem>> ) factory.CreateConverter(
				typeof(List<FileIndexItem>), options);
		// write null list
		using ( var ms = new MemoryStream() )
		using ( var writer = new Utf8JsonWriter(ms) )
		{
			listConv.Write(writer, null!, options);
			writer.Flush();
			var json = Encoding.UTF8.GetString(ms.ToArray());
			Assert.AreEqual("null", json);
		}

		// read null
		var readerNull = new Utf8JsonReader("null"u8);
		readerNull.Read();
		var readNull = listConv.Read(ref readerNull, typeof(List<FileIndexItem>), options);
		Assert.IsNull(readNull);

		// read wrong token
		var readerWrong = new Utf8JsonReader("{}"u8);
		readerWrong.Read();
		try
		{
			listConv.Read(ref readerWrong, typeof(List<FileIndexItem>), options);
			Assert.Fail("Expected JsonException");
		}
		catch ( JsonException )
		{
			// expected
		}

		// valid read
		var jsonValid = "[{\"id\":5,\"FileName\":\"x.jpg\"}]";
		var readerValid = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonValid));
		readerValid.Read();
		var list = listConv.Read(ref readerValid, typeof(List<FileIndexItem>), options);
		Assert.IsNotNull(list);
		Assert.HasCount(1, list);
		Assert.AreEqual(5, list[0].Id);

		// Array converter
		var arrayConv =
			( JsonConverter<FileIndexItem[]> ) factory.CreateConverter(typeof(FileIndexItem[]),
				options);
		using ( var ms2 = new MemoryStream() )
		using ( var writer2 = new Utf8JsonWriter(ms2) )
		{
			arrayConv.Write(writer2, null!, options);
			writer2.Flush();
			var json = Encoding.UTF8.GetString(ms2.ToArray());
			Assert.AreEqual("null", json);
		}

		var readerArrayValid =
			new Utf8JsonReader("[{\"id\":6,\"FileName\":\"y.jpg\"}]"u8);
		readerArrayValid.Read();
		var arr = arrayConv.Read(ref readerArrayValid, typeof(FileIndexItem[]), options);
		Assert.IsNotNull(arr);
		Assert.HasCount(1, arr);
		Assert.AreEqual(6, arr[0].Id);
	}

	[TestMethod]
	public void CreateConverter_ForIEnumerableAndIList_Works()
	{
		var factory = new FileIndexItemWithIdJsonConverterFactory();
		var options = new JsonSerializerOptions();

		// IEnumerable<FileIndexItem>
		var enumConvObj = factory.CreateConverter(typeof(IEnumerable<FileIndexItem>), options);
		var enumConv =
			( JsonConverter<List<FileIndexItem>> )
			enumConvObj; // underlying converter is ListFileIndexItemConverter

		var readerValidEnum =
			new Utf8JsonReader("[{\"id\":7,\"FileName\":\"z.jpg\"}]"u8);
		readerValidEnum.Read();
		var listEnum = enumConv.Read(ref readerValidEnum, typeof(List<FileIndexItem>), options);
		Assert.IsNotNull(listEnum);
		Assert.HasCount(1, listEnum);
		Assert.AreEqual(7, listEnum[0].Id);

		// IList<FileIndexItem>
		var ilistConvObj = factory.CreateConverter(typeof(IList<FileIndexItem>), options);
		var ilistConv = ( JsonConverter<List<FileIndexItem>> ) ilistConvObj;

		var readerValidIList =
			new Utf8JsonReader("[{\"id\":8,\"FileName\":\"w.jpg\"}]"u8);
		readerValidIList.Read();
		var listIList = ilistConv.Read(ref readerValidIList, typeof(List<FileIndexItem>), options);
		Assert.IsNotNull(listIList);
		Assert.HasCount(1, listIList);
		Assert.AreEqual(8, listIList[0].Id);
	}

	[TestMethod]
	public void Read_NullToken_ReturnsNull_ForListAndArray()
	{
		var factory = new FileIndexItemWithIdJsonConverterFactory();
		var options = new JsonSerializerOptions();

		var listConv =
			( JsonConverter<List<FileIndexItem>> ) factory.CreateConverter(
				typeof(List<FileIndexItem>), options);
		var arrayConv =
			( JsonConverter<FileIndexItem[]> ) factory.CreateConverter(typeof(FileIndexItem[]),
				options);

		var readerListNull = new Utf8JsonReader("null"u8);
		readerListNull.Read();
		var listNull = listConv.Read(ref readerListNull, typeof(List<FileIndexItem>), options);
		Assert.IsNull(listNull);

		var readerArrayNull = new Utf8JsonReader("null"u8);
		readerArrayNull.Read();
		var arrNull = arrayConv.Read(ref readerArrayNull, typeof(FileIndexItem[]), options);
		Assert.IsNull(arrNull);
	}
}
