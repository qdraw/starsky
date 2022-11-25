using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.database.Interfaces;
using starsky.foundation.storage.Services;

namespace starsky.Middleware;

public class TestMiddleware
{
	       
	public TestMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	private readonly RequestDelegate _next;
	private readonly IQuery _query;

	public async Task Invoke(HttpContext context)
	{
		if ( context.Request.Path.Value != "/" )
		{
			await _next.Invoke(context);
			return;
		}

		if ( context.Request.Method.ToLowerInvariant() == "get" || context.Request.Method.ToLowerInvariant() == "options")
		{
			context.Response.Headers.Add("DAV", "1,2, access-control");
			context.Response.Headers.Add("MS-Author-Via", "DAV");
			context.Response.Headers.Add("WWW-Authenticate",$"WWW-Authenticate: Basic realm=\"server\"");
			context.Response.StatusCode = 401;
			await context.Response.BodyWriter.WriteAsync(Array.Empty<byte>());
			return;
		}

		if ( context.Request.Method.ToLowerInvariant() == "head"  )
		{
			var login = await BasicAuthenticationMiddleware.Authenticate(context);
			context.Response.StatusCode = login == false ? 401 : 200;
			
			await context.Response.BodyWriter.WriteAsync(Array.Empty<byte>());
			return;
		}

		if ( context.Request.Method.ToLowerInvariant() == "propfind" && context.Request.ContentLength != 0 && context.Request.ContentType?.Contains("xml") == true )
		{
			context.Request.EnableBuffering();
			var bodyAsText = await new System.IO.StreamReader(context.Request.Body).ReadToEndAsync();
			context.Request.Body.Position = 0;
			
			XmlDocument gpxDoc = new XmlDocument();
			gpxDoc.LoadXml(bodyAsText);


			Console.WriteLine();
			// <?xml version="1.0" encoding="UTF-8" standalone="yes"?><propfind xmlns="DAV:"><prop><creationdate/><displayname/><getcontentlength/><getcontenttype/><getetag/><getlastmodified/><lockdiscovery/><resourcetype/><s:lastmodified_server xmlns:s="SAR:"/><s:lastmodified xmlns:s="SAR:"/></prop></propfind>
			
			//await _query.GetAllObjectsAsync("/");
		}
		
	}
}
