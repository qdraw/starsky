using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using starsky.Attributes;
using starsky.foundation.database.Models;
using starsky.foundation.http.Interfaces;
using starsky.foundation.http.Streaming;
using starsky.foundation.import.Helpers;
using starsky.foundation.import.Interfaces;
using starsky.foundation.import.Models;
using starsky.foundation.import.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.Controllers;

[Authorize] // <- should be logged in!
[SuppressMessage("Usage", "S5693:Make sure the content " +
                          "length limit is safe here",
	Justification = "Is checked")]
public sealed class ImportController : Controller
{
	private readonly AppSettings _appSettings;
	private readonly IUpdateBackgroundTaskQueue _bgTaskQueue;
	private readonly IStorage _hostFileSystemStorage;
	private readonly IHttpClientHelper _httpClientHelper;
	private readonly IImport _import;
	private readonly IChunkUploadSessionStore _chunkUploadSessionStore;
	private readonly IWebLogger _logger;
	private readonly ISelectorStorage _selectorStorage;

	public ImportController(IImport import, AppSettings appSettings,
		IUpdateBackgroundTaskQueue queue,
		IHttpClientHelper httpClientHelper, ISelectorStorage selectorStorage, IWebLogger logger,
		IChunkUploadSessionStore? chunkUploadSessionStore = null)
	{
		_appSettings = appSettings;
		_import = import;
		_bgTaskQueue = queue;
		_httpClientHelper = httpClientHelper;
		_selectorStorage = selectorStorage;
		_hostFileSystemStorage =
			selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		_logger = logger;
		_chunkUploadSessionStore = chunkUploadSessionStore ?? new InMemoryChunkUploadSessionStore();
	}

	[HttpPost("/api/import/chunk/init")]
	[ProducesResponseType(typeof(ChunkUploadInitResultModel), 200)]
	[ProducesResponseType(typeof(string), 400)]
	public IActionResult ImportChunkInit(string fileName, int totalChunks, long totalSize)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		if ( string.IsNullOrWhiteSpace(fileName) || totalChunks <= 0 || totalSize <= 0 )
		{
			return BadRequest("invalid init payload");
		}

		var safeFileName = Path.GetFileName(fileName);
		var result = _chunkUploadSessionStore.Create(safeFileName, string.Empty,
			totalChunks, totalSize);
		return Json(result);
	}

	[HttpPut("/api/import/chunk/{uploadId}")]
	[RequestSizeLimit(120_000_000)]
	[ProducesResponseType(typeof(ChunkUploadStatusModel), 200)]
	[ProducesResponseType(typeof(string), 400)]
	[ProducesResponseType(typeof(string), 404)]
	public async Task<IActionResult> ImportChunkPart(string uploadId, int chunkIndex)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		using var memoryStream = new MemoryStream();
		await Request.Body.CopyToAsync(memoryStream);
		var chunkData = memoryStream.ToArray();

		if ( chunkData.Length == 0 )
		{
			return BadRequest("chunk is empty");
		}

		if ( !_chunkUploadSessionStore.AddChunk(uploadId, chunkIndex, chunkData,
			     out var errorMessage) )
		{
			if ( errorMessage == "upload session not found" )
			{
				return NotFound(errorMessage);
			}

			return BadRequest(errorMessage);
		}

		var status = _chunkUploadSessionStore.GetStatus(uploadId);
		return Json(status);
	}

	[HttpGet("/api/import/chunk/{uploadId}/status")]
	[ProducesResponseType(typeof(ChunkUploadStatusModel), 200)]
	[ProducesResponseType(typeof(string), 404)]
	public IActionResult ImportChunkStatus(string uploadId)
	{
		var status = _chunkUploadSessionStore.GetStatus(uploadId);
		if ( status == null )
		{
			return NotFound("upload session not found");
		}

		return Json(status);
	}

	[HttpDelete("/api/import/chunk/{uploadId}")]
	public IActionResult DeleteImportChunk(string uploadId)
	{
		_chunkUploadSessionStore.Delete(uploadId);
		return NoContent();
	}

	[HttpPost("/api/import/chunk/{uploadId}/complete")]
	[ProducesResponseType(typeof(List<ImportIndexItem>), 200)]
	[ProducesResponseType(typeof(string), 400)]
	[ProducesResponseType(typeof(string), 404)]
	public async Task<IActionResult> CompleteImportChunk(string uploadId)
	{
		var status = _chunkUploadSessionStore.GetStatus(uploadId);
		if ( status == null )
		{
			return NotFound("upload session not found");
		}

		if ( !_chunkUploadSessionStore.TryAssemble(uploadId, out var payload,
			     out var errorMessage) )
		{
			return BadRequest(errorMessage);
		}

		var chunkImportDirectory = Path.Combine(_appSettings.TempFolder, "chunk-import", uploadId);
		_hostFileSystemStorage.CreateDirectory(chunkImportDirectory);
		var tempImportPath = Path.Combine(chunkImportDirectory, status.FileName);

		var writeStatus = await _hostFileSystemStorage.WriteStreamAsync(new MemoryStream(payload),
			tempImportPath);
		if ( !writeStatus )
		{
			return BadRequest("unable to persist assembled file");
		}

		var fileIndexResultsList = await ProcessImportTempImportPaths(
			new List<string> { tempImportPath }, new ImportSettingsModel(Request));

		_chunkUploadSessionStore.Delete(uploadId);
		return Json(fileIndexResultsList);
	}

	/// <summary>
	///     Import a file using the structure format
	/// </summary>
	/// <returns>the ImportIndexItem of the imported files</returns>
	/// <response code="200">done</response>
	/// <response code="206">file already imported</response>
	/// <response code="415">Wrong input (e.g. wrong extenstion type)</response>
	[HttpPost("/api/import")]
	[DisableFormValueModelBinding]
	[Produces("application/json")]
	[RequestFormLimits(MultipartBodyLengthLimit = 320_000_000)]
	[RequestSizeLimit(320_000_000)] // in bytes, 305MB
	[ProducesResponseType(typeof(List<ImportIndexItem>), 200)] // yes
	[ProducesResponseType(typeof(List<ImportIndexItem>),
		206)] // When all items are already imported
	[ProducesResponseType(typeof(List<ImportIndexItem>),
		415)] // Wrong input (e.g. wrong extenstion type)
	public async Task<IActionResult> IndexPost() // aka ActionResult Import
	{
		var tempImportPaths = await Request.StreamFile(_appSettings, _selectorStorage);
		var importSettings = new ImportSettingsModel(Request);
		var fileIndexResultsList = await ProcessImportTempImportPaths(tempImportPaths, importSettings);
		return Json(fileIndexResultsList);
	}

	private async Task<List<ImportIndexItem>> ProcessImportTempImportPaths(
		List<string> tempImportPaths, ImportSettingsModel importSettings)
	{
		var fileIndexResultsList = await _import.Preflight(tempImportPaths, importSettings);

		// Import files >
		var payload = new ImportBackgroundPayload
		{
			TempImportPaths = tempImportPaths,
			ImportSettings = importSettings,
			IsVerbose = _appSettings.IsVerbose()
		};
		await _bgTaskQueue.QueueJobAsync(new BackgroundTaskQueueJob
		{
			MetaData = string.Join(",", tempImportPaths),
			TraceParentId = Activity.Current?.Id,
			PriorityLane = ProcessTaskQueue.PriorityLaneUpdate,
			JobType = ImportBackgroundJobHandler.Import,
			PayloadJson = JsonSerializer.Serialize(payload)
		});

		// When all items are already imported
		if ( importSettings.IndexMode &&
		     fileIndexResultsList.TrueForAll(p => p.Status != ImportStatus.Ok) )
		{
			Response.StatusCode = 206;
		}

		// Wrong input (extension is not allowed)
		// Only treat as wrong input when there are items and all of them are FileError.
		// An empty preflight list should not be considered a wrong input (it may mean nothing to import)
		if ( fileIndexResultsList.Count > 0 && fileIndexResultsList.TrueForAll(p => p.Status == ImportStatus.FileError) )
		{
			_logger.LogDebug("Wrong input");
			Response.StatusCode = 415;
		}

		return fileIndexResultsList;
	}


	/// <summary>
	///     Import file from web-url (only whitelisted domains) and import this file into the application
	/// </summary>
	/// <param name="fileUrl">the url</param>
	/// <param name="filename">the filename (optional, random used if empty)</param>
	/// <param name="structure">use structure (optional)</param>
	/// <returns></returns>
	/// <response code="200">done</response>
	/// <response code="206">file already imported</response>
	/// <response code="404">the file url is not found or the domain is not whitelisted</response>
	[HttpPost("/api/import/fromUrl")]
	[ProducesResponseType(typeof(List<ImportIndexItem>), 200)] // yes
	[ProducesResponseType(typeof(List<ImportIndexItem>), 206)] // file already imported
	[ProducesResponseType(404)] // url 404
	[Produces("application/json")]
	public async Task<IActionResult> FromUrl(string fileUrl, string? filename, string structure)
	{
		if ( !ModelState.IsValid )
		{
			return BadRequest("Model invalid");
		}

		filename ??= Base32.Encode(FileHash.GenerateRandomBytes(8)) + ".unknown";

		// I/O function calls should not be vulnerable to path injection attacks
		if ( !Regex.IsMatch(filename, "^[a-zA-Z0-9_\\s\\.]+$",
			     RegexOptions.None, TimeSpan.FromMilliseconds(100)) ||
		     !FilenamesHelper.IsValidFileName(filename) )
		{
			return BadRequest();
		}

		var tempImportFullPath = Path.Combine(_appSettings.TempFolder, filename);
		var importSettings = new ImportSettingsModel(Request) { Structure = structure };
		var isDownloaded = await _httpClientHelper.Download(fileUrl, tempImportFullPath);
		if ( !isDownloaded )
		{
			return NotFound("'file url' not found or domain not allowed " + fileUrl);
		}

		var importedFiles =
			await _import.Importer(new List<string> { tempImportFullPath }, importSettings);
		new RemoveTempAndParentStreamFolderHelper(_hostFileSystemStorage, _appSettings)
			.RemoveTempAndParentStreamFolder(tempImportFullPath);

		if ( importedFiles.Count == 0 )
		{
			Response.StatusCode = 206;
		}

		return Json(importedFiles);
	}
}
