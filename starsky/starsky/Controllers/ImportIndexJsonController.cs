using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.foundation.import.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.Controllers;

[Authorize(Roles = nameof(AccountRoles.AppAccountRoles.Administrator))]
public sealed class ImportIndexJsonController(
	AppSettings appSettings,
	IImportIndexJsonService importIndexJsonService)
	: Controller
{
	[HttpPost("/api/import/index-json/import")]
	[Consumes("application/json")]
	[Produces("application/json")]
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
		var tempPath = GetTempPath(appSettings.TempFolder);
		await System.IO.File.WriteAllTextAsync(tempPath, jsonPayload);

		try
		{
			var result = await importIndexJsonService.ImportAsync(tempPath);
			return Json(result);
		}
		finally
		{
			TryDeleteFile(tempPath);
		}
	}

	[HttpGet("/api/import/index-json/export")]
	[Produces("application/json")]
	public async Task<IActionResult> Export()
	{
		var tempPath = GetTempPath(appSettings.TempFolder);
		var exportPath = await importIndexJsonService.ExportAsync(tempPath);
		try
		{
			if ( !System.IO.File.Exists(exportPath) )
			{
				return NotFound("Export file could not be created");
			}

			var jsonPayload = await System.IO.File.ReadAllTextAsync(exportPath);
			return Content(jsonPayload, "application/json");
		}
		finally
		{
			TryDeleteFile(exportPath);
		}
	}

	private static string GetTempPath(string tempFolder)
	{
		Directory.CreateDirectory(tempFolder);
		return Path.Combine(tempFolder, $"import-index-json-{Guid.NewGuid():N}.json");
	}

	private static void TryDeleteFile(string filePath)
	{
		if ( System.IO.File.Exists(filePath) )
		{
			System.IO.File.Delete(filePath);
		}
	}
}
