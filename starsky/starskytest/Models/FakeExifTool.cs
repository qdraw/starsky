using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.writemeta.Interfaces;
using starskycore.Helpers;
using starskycore.Interfaces;
using starskycore.Models;

namespace starskytest.Models
{
    public class FakeExifTool : IExifTool, IExifToolHostStorage
    {
	    private AppSettings _appSettings;
	    private readonly IStorage _iStorage;

	    public FakeExifTool(IStorage iStorage, AppSettings appSettings)
	    {
		    _appSettings = appSettings;
		    _iStorage = iStorage;
	    }
	    
	    private const string xmpInjection = "<x:xmpmeta xmlns:x=\'adobe:ns:meta/\' x:xmptk=\'Image::ExifTool 11.30\'>" +
	                                        "\n<rdf:RDF xmlns:rdf=\'http://www.w3.org/1999/02/22-rdf-syntax-ns#\'>\n" + 
	                                        "\n <rdf:Description rdf:about=\'\'\n  xmlns:dc=\'http://purl.org/dc/elements/1.1/\'>\n  <dc:subject>\n " +
	                                        "  <rdf:Bag>\n    " + "<rdf:li>test</rdf:li>\n   </rdf:Bag>\n  </dc:subject>\n </rdf:Description>\n\n" +
	                                        " <rdf:Description rdf:about=\'\'\n " + " xmlns:pdf=\'http://ns.adobe.com/pdf/1.3/\'>\n  " +
	                                        "<pdf:Keywords>kamer</pdf:Keywords>\n </rdf:Description>\n</rdf:RDF>\n</x:xmpmeta>\n";

	    public Task<bool> WriteTagsAsync(string subPath, string command)
		{

			if ( subPath.EndsWith(".xmp") )
			{
				var stream = new PlainTextFileHelper().StringToStream(xmpInjection);
				_iStorage.WriteStream(stream, subPath);
			}
			
			Console.WriteLine("Fake ExifTool + " + subPath + " " + command);
			
			return Task.FromResult(true);
	    }

	    public Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
	    {
		    return Task.FromResult(true);
	    }
    }
}
