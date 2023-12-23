using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using starsky.foundation.georealtime.Helpers;
using starsky.foundation.georealtime.Interfaces;
using starsky.foundation.georealtime.Models;
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
		if ( !readString.Key )
		{
			return;
		}

		var xDocument = XmlParse(readString.Value);
		var result = Kml2IntermediateModelConverter.ParseKml(xDocument);
		var model = IntermediateModelConverter.Covert2GeoJson(result, true);
	}

	private static XDocument XmlParse(string content)
	{
		return XDocument.Parse(content);
	}


}
