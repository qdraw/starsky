﻿using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.writemeta.Interfaces;
using starsky.foundation.writemeta.Models;

namespace starskytest.FakeMocks;

public sealed class FakeExifTool : IExifToolHostStorage
{
	public const string XmpInjection =
		"<x:xmpmeta xmlns:x=\'adobe:ns:meta/\' x:xmptk=\'Image::ExifTool 11.30\'>" +
		"\n<rdf:RDF xmlns:rdf=\'http://www.w3.org/1999/02/22-rdf-syntax-ns#\'>\n" +
		"\n <rdf:Description rdf:about=\'\'\n  xmlns:dc=\'http://purl.org/dc/elements/1.1/\'>\n  <dc:subject>\n " +
		"  <rdf:Bag>\n    " +
		"<rdf:li>test</rdf:li>\n   </rdf:Bag>\n  </dc:subject>\n </rdf:Description>\n\n" +
		" <rdf:Description rdf:about=\'\'\n " +
		" xmlns:pdf=\'http://ns.adobe.com/pdf/1.3/\'>\n  " +
		"<pdf:Keywords>kamer</pdf:Keywords>\n </rdf:Description>\n</rdf:RDF>\n</x:xmpmeta>\n";

	private readonly IStorage _iStorage;

	// ReSharper disable once UnusedParameter.Local
	public FakeExifTool(IStorage iStorage, AppSettings _)
	{
		_iStorage = iStorage;
	}

	public async Task<bool> WriteTagsAsync(string subPath, string command)
	{
		Console.WriteLine("Fake ExifTool + " + subPath + " " + command);

		if ( subPath.EndsWith(".xmp") )
		{
			var stream = StringToStreamHelper.StringToStream(XmpInjection);
			await _iStorage.WriteStreamAsync(stream, subPath);
		}

		return true;
	}

	public async Task<ExifToolWriteTagsAndRenameThumbnailModel> WriteTagsAndRenameThumbnailAsync(
		string subPath, string? beforeFileHash,
		string command, CancellationToken cancellationToken = default)
	{
		Console.WriteLine("Fake ExifTool + " + subPath + " " + command);

		if ( subPath.EndsWith(".xmp") )
		{
			var stream = StringToStreamHelper.StringToStream(XmpInjection);
			await _iStorage.WriteStreamAsync(stream, subPath);
		}

		var newFileHash =
			( await new FileHash(_iStorage, new FakeIWebLogger()).GetHashCodeAsync(subPath) ).Key;
		return new ExifToolWriteTagsAndRenameThumbnailModel(true, newFileHash);
	}

	public Task<bool> WriteTagsThumbnailAsync(string fileHash, string command)
	{
		return Task.FromResult(true);
	}
}
