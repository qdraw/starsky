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
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.foundation.georealtime.Services;

public sealed class KmlImport : IKmlImport
{
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly AppSettings _appSettings;
	private readonly IStorage _hostStorage;
	private readonly IStorage _subStorage;

	public KmlImport(IHttpClientHelper httpClientHelper, ISelectorStorage selectorStorage, AppSettings appSettings)
	{
		_httpClientHelper = httpClientHelper;
		_appSettings = appSettings;
		_hostStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_subStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.SubPath);
	}
	
	public async Task Import(string kmlPathOrUrl)
	{

		var readString = await _httpClientHelper.ReadString(kmlPathOrUrl);
		if ( !readString.Key )
		{
			// var existFile = _subStorage.ExistFile(kmlPathOrUrl);
			// if ( existFile )
			// {
			// 	readString = _subStorage.ReadStream(kmlPathOrUrl);
			// }
			// else
			// {
			// 	existFile = _hostStorage.ExistFile(kmlPathOrUrl);
			// 	if ( existFile )
			// 	{
			// 		readString = _hostStorage.ReadStream(kmlPathOrUrl);
			// 	}
			// }

			return;
		}

		var xDocument = XmlParseToXDocument(readString.Value);
		var waypoints = Kml2IntermediateModelConverter.ParseKml(xDocument);
		var model = IntermediateModelConverter.Covert2GeoJson(waypoints, true);
		var resultGpx = IntermediateModelConverter.ConvertToGpx(waypoints);

		Console.WriteLine();
	}

	private static XDocument XmlParseToXDocument(string content)
	{
		return XDocument.Parse(content);
	}


}
