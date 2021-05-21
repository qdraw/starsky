using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.JsonConverter;

namespace starskytest.starsky.foundation.platform.JsonConverter
{
	[TestClass]
	public class JsonBoolQuotedConverterTest
	{
		[TestMethod]
		public void Serialize()
		{
			var json = JsonSerializer.Serialize(
				new Dictionary<string, bool>{{
					"key", true
				}}, new JsonSerializerOptions
				{
					Converters =
					{
						new JsonBoolQuotedConverter(),
					}
				});
			
			Assert.AreEqual("{\"key\":\"true\"}", json);
		}
		
		// ReSharper disable once ClassNeverInstantiated.Local
		private class KeyExample
		{
			// ReSharper disable once UnassignedGetOnlyAutoProperty
			// ReSharper disable once UnusedAutoPropertyAccessor.Local
			public bool Key { get; set; }
		}
		
		[TestMethod]
		public void DeserializeQuotedTrue()
		{
			var output = JsonSerializer.Deserialize<KeyExample>(
				"{\"Key\":\"true\"}",
				new JsonSerializerOptions
				{
					Converters = {
						new JsonBoolQuotedConverter(),
					}
				}
			);

			Assert.IsTrue(output.Key);
		}
		
		[TestMethod]
		public void DeserializeQuotedFalse()
		{
			var output = JsonSerializer.Deserialize<KeyExample>(
				"{\"Key\":\"false\"}",
				new JsonSerializerOptions
				{
					Converters = {
						new JsonBoolQuotedConverter(),
					}
				}
			);

			Assert.IsFalse(output.Key);
		}
		
		[TestMethod]
		public void DeserializeNonQuoted()
		{
			var output = JsonSerializer.Deserialize<KeyExample>(
				"{\"Key\":true}",
				new JsonSerializerOptions
				{
					Converters = {
						new JsonBoolQuotedConverter(),
					}
				}
			);

			Assert.IsTrue(output.Key);
		}
	}
}
