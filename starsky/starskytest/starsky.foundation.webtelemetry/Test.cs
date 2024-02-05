using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OpenTelemetry.Proto.Trace.V1;

namespace starskytest.starsky.foundation.webtelemetry;

[TestClass]
public class Test
{
	[TestMethod]
	public void Test1()
	{
        // Your JSON input
        string jsonInput = @"{
            ""resourceSpans"":[
                {
                    ""resource"":{
                        ""attributes"":[
                            {""key"":""service.name"",""value"":{""stringValue"":""starsky-client-app""}},
                            {""key"":""telemetry.sdk.language"",""value"":{""stringValue"":""webjs""}},
                            {""key"":""telemetry.sdk.name"",""value"":{""stringValue"":""opentelemetry""}},
                            {""key"":""telemetry.sdk.version"",""value"":{""stringValue"":""1.19.0""}}
                        ],
                        ""droppedAttributesCount"":0
                    },
                    ""scopeSpans"":[
                        {
                            ""scope"":{
                                ""name"":""@opentelemetry/instrumentation-fetch"",
                                ""version"":""0.46.0""
                            },
                            ""spans"":[
                                {
                                    ""traceId"":""0769e5a767e763e308d95481c06dcc23"",
                                    ""spanId"":""15eb16ca02c1dfe3"",
                                    ""name"":""HTTP GET"",
                                    ""kind"":3,
                                    ""startTimeUnixNano"":""1704475991553000000"",
                                    ""endTimeUnixNano"":""1704475991795000000"",
                                    ""attributes"":[
                                        {""key"":""component"",""value"":{""stringValue"":""fetch""}},
                                        {""key"":""http.method"",""value"":{""stringValue"":""GET""}},
                                        {""key"":""http.url"",""value"":{""stringValue"":""http://localhost:5173/starsky/api/index?""}},
                                        {""key"":""http.status_code"",""value"":{""intValue"":200}},
                                        {""key"":""http.status_text"",""value"":{""stringValue"":""OK""}},
                                        {""key"":""http.host"",""value"":{""stringValue"":""localhost:5173""}},
                                        {""key"":""http.scheme"",""value"":{""stringValue"":""http""}},
                                        {""key"":""http.user_agent"",""value"":{""stringValue"":""Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:121.0) Gecko/20100101 Firefox/121.0""}},
                                        {""key"":""http.response_content_length"",""value"":{""intValue"":1883}},
                                        {""key"":""http.response_content_length_uncompressed"",""value"":{""intValue"":10379}}
                                    ],
                                    ""droppedAttributesCount"":0,
                                    ""events"":[
                                        {""attributes"":[],""name"":""fetchStart"",""timeUnixNano"":""1704475991554000000"",""droppedAttributesCount"":0},
                                        {""attributes"":[],""name"":""domainLookupStart"",""timeUnixNano"":""1704475991554000000"",""droppedAttributesCount"":0},
                                        {""attributes"":[],""name"":""domainLookupEnd"",""timeUnixNano"":""1704475991554000000"",""droppedAttributesCount"":0},
                                        {""attributes"":[],""name"":""connectStart"",""timeUnixNano"":""1704475991554000000"",""droppedAttributesCount"":0},
                                        {""attributes"":[],""name"":""connectEnd"",""timeUnixNano"":""1704475991554000000"",""droppedAttributesCount"":0},
                                        {""attributes"":[],""name"":""requestStart"",""timeUnixNano"":""1704475991777000000"",""droppedAttributesCount"":0},
                                        {""attributes"":[],""name"":""responseStart"",""timeUnixNano"":""1704475991795000000"",""droppedAttributesCount"":0},
                                        {""attributes"":[],""name"":""responseEnd"",""timeUnixNano"":""1704475991795000000"",""droppedAttributesCount"":0}
                                    ],
                                    ""droppedEventsCount"":0,
                                    ""status"":{""code"":0},
                                    ""links"":[],
                                    ""droppedLinksCount"":0
                                }
                            ]
                        }
                    ]
                }
            ]
        }";

        // Deserialize JSON to C# object
        var tracesData = JsonConvert.DeserializeObject<TracesData>(jsonInput);

        // Now you have a C# object, you can perform additional processing or convert it to other formats
        // For demonstration, let's convert it back to Protobuf binary
        byte[] protobufData = ConvertToProtobuf(tracesData);

        Console.WriteLine("Protobuf binary data:");

        // You can do something with the Protobuf binary data, send it over the network, etc.
    }

	private static byte[] ConvertToProtobuf(TracesData tracesData)
	{
		using (var stream = new System.IO.MemoryStream())
		{
			// Serialize the TracesData object to Protobuf binary
			ProtoBuf.Serializer.Serialize(stream, tracesData);

			// Get the binary data
			return stream.ToArray();
		}
	}
}
