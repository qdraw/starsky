using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;

namespace starsky.Helpers;

[Service(typeof(IHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class OpenApiExportHelper : BackgroundService
{
	private readonly IWebLogger? _logger;
	private readonly IServiceScopeFactory _serviceScopeFactory;

	public OpenApiExportHelper(IServiceScopeFactory serviceScopeFactory, IWebLogger? logger = null)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;
	}

	/// <summary>
	///     Running scoped services
	///     @see: https://thinkrethink.net/2018/07/12/injecting-a-scoped-service-into-ihostedservice/
	/// </summary>
	/// <param name="stoppingToken">Cancellation Token, but it ignored</param>
	/// <returns>CompletedTask</returns>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var appSettings = scope.ServiceProvider.GetRequiredService<AppSettings>();
		if ( appSettings.AddOpenApi != true || appSettings.AddOpenApiExport != true )
		{
			return;
		}

		var selectorStorage = scope.ServiceProvider.GetRequiredService<ISelectorStorage>();
		// AddOpenApi registers the provider per document name, lowercased
		var documentProvider = scope.ServiceProvider
			.GetRequiredKeyedService<IOpenApiDocumentProvider>(appSettings.Name.ToLowerInvariant());
		var applicationLifetime =
			scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();

		await Add03AppExport(appSettings, selectorStorage, documentProvider);
		Add04OpenApiExportExitAfter(appSettings, applicationLifetime);
	}

	/// <summary>
	///     to run ExecuteAsync from outside this class
	/// </summary>
	/// <returns></returns>
	internal Task ExecuteAsync()
	{
		return ExecuteAsync(CancellationToken.None);
	}

	/// <summary>
	///     Export Values to Storage
	/// </summary>
	/// <param name="appSettings">App Settings</param>
	/// <param name="selectorStorage">Storage Provider</param>
	/// <param name="documentProvider">OpenAPI document provider</param>
	/// <exception cref="ArgumentException">openApiJsonText = null</exception>
	public async Task<bool> Add03AppExport(AppSettings appSettings,
		ISelectorStorage selectorStorage, IOpenApiDocumentProvider? documentProvider)
	{
		if ( appSettings.AddOpenApi != true || appSettings.AddOpenApiExport != true )
		{
			return false;
		}

		var openApiJsonText = await GenerateOpenApi(documentProvider);
		if ( string.IsNullOrEmpty(openApiJsonText) )
		{
			throw new ArgumentException("openApiJsonText = null", nameof(documentProvider));
		}

		var openApiJsonFullPath =
			Path.Join(appSettings.TempFolder, appSettings.Name.ToLowerInvariant() + ".json");

		var storage = selectorStorage.Get(SelectorStorage.StorageServices.HostFilesystem);
		storage.FileDelete(openApiJsonFullPath);
		await storage.WriteStreamAsync(StringToStreamHelper.StringToStream(openApiJsonText),
			openApiJsonFullPath);

		_logger?.LogInformation($"app__addOpenApiExport {openApiJsonFullPath}");
		return true;
	}

	public bool Add04OpenApiExportExitAfter(AppSettings appSettings,
		IHostApplicationLifetime applicationLifetime)
	{
		if ( appSettings.AddOpenApi != true ||
		     appSettings.AddOpenApiExport != true ||
		     appSettings.AddOpenApiExportExitAfter != true )
		{
			return false;
		}

		applicationLifetime.StopApplication();
		_logger?.LogInformation("App is stopped");
		return true;
	}

	/// <summary>
	///     Generate OpenAPI as Json
	///     Set this:
	///     app__AddOpenApi = true
	///     app__AddOpenApiExport = true
	/// </summary>
	/// <param name="documentProvider">OpenAPI document provider</param>
	/// <returns>json or empty when no provider</returns>
	internal static async Task<string> GenerateOpenApi(IOpenApiDocumentProvider? documentProvider)
	{
		if ( documentProvider == null )
		{
			return string.Empty;
		}

		var document = await documentProvider.GetOpenApiDocumentAsync();
		var stringWriter = new StringWriter();
		document.SerializeAsV3(new OpenApiJsonWriter(stringWriter));
		return stringWriter.ToString();
	}
}
