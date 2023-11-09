using starsky.foundation.georealtime.Interfaces;
using starsky.foundation.http.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.georealtime.Services;

public sealed class KmlImport : IKmlImport
{
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IStorage _hostStorage;

	public KmlImport(IHttpClientHelper httpClientHelper, ISelectorStorage selectorStorage)
	{
		_httpClientHelper = httpClientHelper;
		_hostStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
	}
	
	public async Task Import(string kmlPathOrUrl)
	{

		var readString = await _httpClientHelper.ReadString(kmlPathOrUrl);
		readString.Key
	}
}
