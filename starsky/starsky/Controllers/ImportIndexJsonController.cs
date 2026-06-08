using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.import.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Controllers;

[Authorize(Roles = nameof(AccountRoles.AppAccountRoles.Administrator))]
public sealed class ImportIndexJsonController(
	IImportIndexJsonService importIndexJsonService,
	ISelectorStorage selectorStorage)
	: Controller
{
	[HttpPost("/api/import/index-json/import")]
	[Consumes("application/json")]
	[Produces("application/json")]
	[RequestSizeLimit(100_000_000)] // in bytes, 100MB
	public async Task<IActionResult> Import([FromBody] JsonElement importJson)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("ModelState is not valid");
		}

		if ( importJson.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined )
		{
			return BadRequest("json payload is required");
		}

		var jsonPayload = importJson.GetRawText();
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
		var tempPath = GetTempFileName();

		await using var jsonStream = StringToStreamHelper.StringToStream(jsonPayload);
		await storage.WriteStreamAsync(jsonStream, tempPath);

		try
		{
			var result = await importIndexJsonService.ImportAsync(tempPath, 
				SelectorStorage.StorageServices.Temporary);
			return Json(result);
		}
		finally
		{
			storage.FileDelete(tempPath);
		}
	}

	[HttpGet("/api/import/index-json/export")]
	[Produces("application/json")]
	public async Task<IActionResult> Export()
	{
		var storage = selectorStorage.Get(SelectorStorage.StorageServices.Temporary);
		var tempPath = GetTempFileName();
		var exportPath = await importIndexJsonService.ExportAsync(tempPath, 
			SelectorStorage.StorageServices.Temporary);
		try
		{
			if ( !storage.ExistFile(exportPath) )
			{
				return NotFound("Export file could not be created");
			}

			var jsonPayload =
				await StreamToStringHelper.StreamToStringAsync(storage.ReadStream(exportPath));
			return Content(jsonPayload, "application/json");
		}
		finally
		{
			storage.FileDelete(tempPath);
		}
	}

	private static string GetTempFileName()
	{
		return $"import-index-json-{Guid.NewGuid():N}.json";
	}
}
