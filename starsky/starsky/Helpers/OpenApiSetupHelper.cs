using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using starsky.foundation.platform.Models;

namespace starsky.Helpers;

public sealed class OpenApiSetupHelper(AppSettings appSettings)
{
	public void Add01OpenApiHelper(IServiceCollection services)
	{
		// Only [ApiController] classes are visible to ApiExplorer by default, but most controllers
		// here derive from Controller. Swashbuckle used to make all of them visible
		services.Configure<MvcOptions>(options =>
			options.Conventions.Add(new ApiExplorerVisibleConvention()));

		var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

		// XML comments are picked up at compile time (GenerateDocumentationFile)
		services.AddOpenApi(appSettings.Name, options =>
		{
			options.AddDocumentTransformer((document, _, _) =>
			{
				document.Info = new OpenApiInfo { Title = appSettings.Name, Version = version };
				document.Components ??= new OpenApiComponents();
				document.Components.SecuritySchemes ??=
					new Dictionary<string, IOpenApiSecurityScheme>();
				document.Components.SecuritySchemes["basic"] = new OpenApiSecurityScheme
				{
					Type = SecuritySchemeType.Http, Scheme = "basic"
				};
				document.Security ??= [];
				document.Security.Add(new OpenApiSecurityRequirement
				{
					{ new OpenApiSecuritySchemeReference("basic", document, null), [] }
				});
				return Task.CompletedTask;
			});
		});
	}

	/// <summary>
	///     Expose the OpenAPI document to `/openapi/{Name}.json`
	///     and optional the Scalar UI to the `/scalar` endpoint
	/// </summary>
	/// <param name="app"></param>
	public void Add02AppUseOpenApiAndScalar(IApplicationBuilder app)
	{
		// Use OpenApi only when enabled, default false
		// recommend to disable in production
		// AddScalar implies AddOpenApi, the UI is useless without the document
		if ( appSettings.AddOpenApi != true && appSettings.AddScalar != true )
		{
			return;
		}

		// Own endpoint block, so the docs are served before the account/auth middleware.
		// Limited to the docs paths: the first UseEndpoints runs every matched endpoint,
		// including controllers, and those need the authorization middleware that comes later
		app.UseWhen(context =>
				context.Request.Path.StartsWithSegments("/openapi") ||
				context.Request.Path.StartsWithSegments("/scalar") ||
				context.Request.Path.StartsWithSegments("/swagger"),
			branch => branch.UseEndpoints(endpoints =>
			{
				endpoints.MapOpenApi();

				if ( appSettings.AddScalar != true )
				{
					return;
				}

				// bookmarks and links to the former Swagger UI
				// the path is bound only to satisfy the route analyzer; every swagger url
				// goes to the same place
				endpoints.MapGet("/swagger/{**path}", (HttpContext context, string? path) =>
					Results.Redirect($"{context.Request.PathBase}/scalar/{path}"));
				
				endpoints.MapScalarApiReference(options =>
				{
					options.WithTitle(appSettings.Name);
					options.AddDocument(appSettings.Name.ToLowerInvariant());
				});
			}));
	}

	/// <summary>
	///     Make attribute-routed actions visible in the OpenAPI document, unless
	///     they opt-out with [ApiExplorerSettings(IgnoreApi = true)]
	///     (ApiExplorer throws on conventional routes, e.g. the Home controller)
	/// </summary>
	private sealed class ApiExplorerVisibleConvention : IApplicationModelConvention
	{
		public void Apply(ApplicationModel application)
		{
			foreach ( var controller in application.Controllers )
			{
				if ( controller.ApiExplorer.IsVisible.HasValue )
				{
					continue;
				}

				var controllerRouted = controller.Selectors.Any(s => s.AttributeRouteModel != null);
				foreach ( var action in controller.Actions.Where(a =>
					         !a.ApiExplorer.IsVisible.HasValue &&
					         ( controllerRouted ||
					           a.Selectors.Any(s => s.AttributeRouteModel != null) )) )
				{
					action.ApiExplorer.IsVisible = true;
				}
			}
		}
	}
}
