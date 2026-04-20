using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.JsonConverters;
using starsky.foundation.database.Models;

namespace starskytest.starsky.foundation.database.JsonConverters;

[TestClass]
public sealed class FileIndexItemWithIdJsonConverterTest
{
    [TestMethod]
    public void Write_WithFactoryAndOtherConverters_WritesIdAndProperties()
    {
        var conv = new FileIndexItemWithIdJsonConverter();
        var item = new FileIndexItem { Id = 5, FileName = "a.jpg", ParentDirectory = "/" };

        var options = new JsonSerializerOptions();
        // include factory and another converter to exercise the safeOptions copy logic
        options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());
        options.Converters.Add(new JsonStringEnumConverter());

        using var ms = new MemoryStream();
        using ( var writer = new Utf8JsonWriter(ms) )
        {
            conv.Write(writer, item, options);
        }

        var json = Encoding.UTF8.GetString(ms.ToArray());
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.IsTrue(root.TryGetProperty("id", out var idProp) && idProp.GetInt32() == 5, json);
        Assert.IsTrue(root.TryGetProperty("FileName", out var fnProp) && fnProp.GetString() == "a.jpg", json);
    }

    [TestMethod]
    public void Read_WithFactoryAndOtherConverters_ParsesProperties()
    {
        var conv = new FileIndexItemWithIdJsonConverter();
        var json = "{\"id\":7,\"FileName\":\"b.jpg\",\"ParentDirectory\":\"/\"}";
        var bytes = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes);
        // advance to the first token (StartObject)
        reader.Read();

        var options = new JsonSerializerOptions();
        options.Converters.Add(new FileIndexItemWithIdJsonConverterFactory());
        options.Converters.Add(new JsonStringEnumConverter());

        var result = conv.Read(ref reader, typeof(FileIndexItem), options);

        Assert.IsNotNull(result);
        Assert.AreEqual("b.jpg", result.FileName);
        Assert.AreEqual("/", result.ParentDirectory);
        // Id is ignored by the POCO [JsonIgnore] so it may not be populated by the internal deserialize
        // Ensure the converter returned an object and populated other properties.
    }

    [TestMethod]
    public void Roundtrip_SerializeDeserialize_PropertiesRemain()
    {
        var item = new FileIndexItem { Id = 10, FileName = "c.jpg", ParentDirectory = "/" };
        var options = DefaultJsonFileIndexJsonSerializer.WithIdConverter;

        var json = JsonSerializer.Serialize(item, options);
        var deserialized = JsonSerializer.Deserialize<FileIndexItem>(json, options);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(item.FileName, deserialized.FileName);
        Assert.AreEqual(item.ParentDirectory, deserialized.ParentDirectory);
        // Id is not round-tripped to the property because FileIndexItem.Id has [JsonIgnore]
    }
}

