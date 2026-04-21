using System;
using System.Collections.Generic;
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
        Assert.ThrowsExactly<NotSupportedException>(() => factory.CreateConverter(typeof(string), options));
    }

    [TestMethod]
    public void InnerConverters_ReadWrite_Behavior()
    {
        var factory = new FileIndexItemWithIdJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // List converter
        var listConv = (JsonConverter<List<FileIndexItem>>)factory.CreateConverter(typeof(List<FileIndexItem>), options);
        // write null list
        using (var ms = new System.IO.MemoryStream())
        using (var writer = new Utf8JsonWriter(ms))
        {
            listConv.Write(writer, null!, options);
            writer.Flush();
            var json = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("null", json);
        }

        // read null
        var readerNull = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes("null"));
        readerNull.Read();
        var readNull = listConv.Read(ref readerNull, typeof(List<FileIndexItem>), options);
        Assert.IsNull(readNull);

        // read wrong token
        var readerWrong = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes("{}"));
        readerWrong.Read();
        try
        {
            listConv.Read(ref readerWrong, typeof(List<FileIndexItem>), options);
            Assert.Fail("Expected JsonException");
        }
        catch (JsonException)
        {
            // expected
        }

        // valid read
        var jsonValid = "[{\"id\":5,\"FileName\":\"x.jpg\"}]";
        var readerValid = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes(jsonValid));
        readerValid.Read();
        var list = listConv.Read(ref readerValid, typeof(List<FileIndexItem>), options);
        Assert.IsNotNull(list);
        Assert.HasCount(1, list);
        Assert.AreEqual(5, list[0].Id);

        // Array converter
        var arrayConv = (JsonConverter<FileIndexItem[]>)factory.CreateConverter(typeof(FileIndexItem[]), options);
        using (var ms2 = new System.IO.MemoryStream())
        using (var writer2 = new Utf8JsonWriter(ms2))
        {
            arrayConv.Write(writer2, null!, options);
            writer2.Flush();
            var json = System.Text.Encoding.UTF8.GetString(ms2.ToArray());
            Assert.AreEqual("null", json);
        }

        var readerArrayValid = new Utf8JsonReader(System.Text.Encoding.UTF8.GetBytes("[{\"id\":6,\"FileName\":\"y.jpg\"}]"));
        readerArrayValid.Read();
        var arr = arrayConv.Read(ref readerArrayValid, typeof(FileIndexItem[]), options);
        Assert.IsNotNull(arr);
        Assert.HasCount(1, arr);
        Assert.AreEqual(6, arr[0].Id);
    }
}

