using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace starskyhybrid
{
    public class HybridFetchBridge
    {
        public async Task<object> HandleApiCallAsync(FetchRequest req)
        {
            // ...existing code...
            if (req == null || string.IsNullOrEmpty(req.Url)) return new { error = "Invalid request" };

            switch ((req.Url, req.Method))
            {
                case ("/api/user", "GET"):
                    return new { name = "Dion", age = 31 };
                case ("/api/login", "POST"):
                    var body = string.IsNullOrEmpty(req.Body) ? default : JsonSerializer.Deserialize<Dictionary<string, string>>(req.Body);
                    var user = body?["username"];
                    var pass = body?["password"];
                    var success = user == "dion" && pass == "test123";
                    return new { success };
                default:
                    return new { error = "Unknown endpoint" };
            }
        }

        public class FetchRequest
        {
            public string Type { get; set; }
            public string Url { get; set; }
            public string Method { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public string Body { get; set; }
        }
    }
}