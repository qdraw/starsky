using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.georealtime.Converter;
using starsky.foundation.georealtime.Models.GeoJson;

namespace starskytest.starsky.foundation.georealtime.Converter
{
    [TestClass]
    public class GeometryBaseModelConverterTests
    {
        private readonly GeometryBaseModelConverter _converter = new GeometryBaseModelConverter();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { Converters = { new GeometryBaseModelConverter() } };

        [TestMethod]
        public void Read_WithValidPointGeometry_ReturnsGeometryPointModel()
        {
            // Arrange
            var jsonString = "{\"type\":\"Point\",\"coordinates\":[5.485941,51.809360,7.263]}";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));

            // Act
            var result = _converter.Read(ref reader, typeof(GeometryBaseModel), _jsonOptions);

            // Assert
            Assert.IsInstanceOfType(result, typeof(GeometryPointModel));
            var pointModel = (GeometryPointModel)result;
            Assert.IsNotNull(pointModel.Coordinates);
            Assert.AreEqual(3, pointModel.Coordinates.Count);
        }

        [TestMethod]
        public void Read_WithValidLineStringGeometry_ReturnsGeometryLineStringModel()
        {
            // Arrange
            var jsonString = "{\"type\":\"LineString\",\"coordinates\":[[5.485941,51.809360,7.263]]}";
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));

            // Act
            var result = _converter.Read(ref reader, typeof(GeometryBaseModel), _jsonOptions);

            // Assert
            Assert.IsInstanceOfType(result, typeof(GeometryLineStringModel));
            var lineStringModel = (GeometryLineStringModel)result;
            Assert.IsNotNull(lineStringModel.Coordinates);
            Assert.AreEqual(1, lineStringModel.Coordinates.Count);
        }

        private GeometryBaseModel ReadJson(string jsonString)
        {
            using var doc = JsonDocument.Parse(jsonString);
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(jsonString));
            return _converter.Read(ref reader, typeof(GeometryBaseModel), _jsonOptions);
        }

        [TestMethod]
        public void Read_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var jsonString = "{\"invalid\":\"json\"}";

            // Act & Assert
            Assert.ThrowsException<JsonException>(() => ReadJson(jsonString));
        }
        
        [TestMethod]
        public void Read_WithInvalidJson_DifferentType_ThrowsJsonException()
        {
	        // Arrange
	        var jsonString = "{\"type\":\"WRONG_TYPE\",\"coordinates\":[[5.485941,51.809360,7.263]]}";

	        // Act & Assert
	        Assert.ThrowsException<JsonException>(() => ReadJson(jsonString));
        }
        
        [TestMethod]
        public void Read_WithInvalidJson_MissingCoordinates_ThrowsJsonException()
        {
	        // Arrange
	        var jsonString = "{\"type\":\"LineString\"}";

	        // Act & Assert
	        Assert.ThrowsException<JsonException>(() => ReadJson(jsonString));
        }
        
                
        [TestMethod]
        public void Read_WithInvalidJson_CoordinatesNoContent_ThrowsJsonException()
        {
	        // Arrange
	        const string jsonString = "{\"type\":\"LineString\",\"coordinates\":null }";

	        // Act & Assert
	        Assert.ThrowsException<JsonException>(() => ReadJson(jsonString));
        }

        [TestMethod]
        public void Read_WithInvalidJson_CoordinatesNumber_ThrowsJsonException()
        {
	        // Arrange
	        const string jsonString = "{\"type\":\"LineString\",\"coordinates\":1 }";

	        // Act & Assert
	        Assert.ThrowsException<JsonException>(() => ReadJson(jsonString));
        }
        
    }
}
