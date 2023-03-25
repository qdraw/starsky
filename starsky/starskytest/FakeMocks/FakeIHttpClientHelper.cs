using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using starsky.foundation.http.Interfaces;
using starsky.foundation.platform.Helpers;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;

namespace starskytest.FakeMocks;

public class FakeIHttpClientHelper : IHttpClientHelper
{
	private readonly Dictionary<string, KeyValuePair<bool, string>> _inputDictionary;
	private readonly IStorage _storage;

	public FakeIHttpClientHelper(IStorage storage, Dictionary<string, KeyValuePair<bool, string>> inputDictionary)
	{
		_storage = storage;
		_inputDictionary = inputDictionary;
	}

	public List<string> UrlsCalled { get; set; } = new List<string>();
	
	public async Task<bool> Download(string sourceHttpUrl, string fullLocalPath,
		int retryAfterInSeconds = 15)
	{
		UrlsCalled.Add(sourceHttpUrl);
		
		var result =
			_inputDictionary.FirstOrDefault(p => p.Key == sourceHttpUrl);

		if ( result.Value.Value == null )
		{
			return false;
		}
		
		var fileByteArray = Base64Helper.TryParse(result.Value.Value);
		
		await _storage.WriteStreamAsync(new MemoryStream(fileByteArray), fullLocalPath );
		return result.Value.Key;
	}

	public Task<KeyValuePair<bool, string>> ReadString(string sourceHttpUrl)
	{
		UrlsCalled.Add(sourceHttpUrl);
		return Task.FromResult(_inputDictionary.FirstOrDefault(p => p.Key == sourceHttpUrl).Value);
	}

	public Task<KeyValuePair<bool, string>> PostString(string sourceHttpUrl, HttpContent httpContent,
		bool verbose = true)
	{
		UrlsCalled.Add(sourceHttpUrl);
		return Task.FromResult(_inputDictionary.FirstOrDefault(p => p.Key == sourceHttpUrl).Value);
	}
}
