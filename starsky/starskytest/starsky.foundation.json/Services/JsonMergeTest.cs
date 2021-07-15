using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using starsky.foundation.json.Services;

namespace starskytest.starsky.foundation.json.Services
{
	[TestClass]
	public class JsonMergeTest
	{
		[TestMethod]
		public void Test()
		{
			string jsonString1 = @"{
		        ""throw"": null,
		        ""duplicate"": null,
		        ""id"": 1,
		        ""xyz"": null,
		        ""nullOverride2"": false,
		        ""nullOverride1"": null,
		        ""william"": ""shakespeare"",
		        ""complex"": {""overwrite"": ""no"", ""type"": ""string"", ""original"": null, ""another"":[]},
		        ""nested"": [7, {""another"": true}],
		        ""nestedObject"": {""another"": true}
			}";

			string jsonString2 = @"{
		        ""william"": ""dafoe"",
		        ""duplicate"": null,
		        ""foo"": ""bar"",
		        ""baz"": {""temp"": 4},
		        ""xyz"": [1, 2, 3],
		        ""nullOverride1"": true,
		        ""nullOverride2"": null,
		        ""nested"": [1, 2, 3, null, {""another"": false}],
		        ""nestedObject"": [""wow""],
		        ""complex"": {""temp"": true, ""overwrite"": ""ok"", ""type"": 14},
		        ""temp"": null
			}";

			JObject jObj1 = JObject.Parse(jsonString1);
			JObject jObj2 = JObject.Parse(jsonString2);

			jObj1.Merge(jObj2);
			jObj2.Merge(JObject.Parse(jsonString1));

			Assert.AreEqual(jObj1.ToString(), JsonMerge.Merge(jsonString1, jsonString2));
			Assert.AreEqual(jObj2.ToString(), JsonMerge.Merge(jsonString2, jsonString1));
		}
	}
}
