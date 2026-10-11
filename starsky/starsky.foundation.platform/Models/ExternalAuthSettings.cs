using System;
using System.Collections.Generic;
using System.Linq;

namespace starsky.foundation.platform.Models;

/// <summary>
///     Optional external authentication configuration
/// </summary>
public class ExternalAuthSettings
{
	/// <summary>
	///     Global feature flag for external authentication
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	///     Configured providers, each provider can also be toggled separately
	/// </summary>
	public List<ExternalAuthProviderSettings> Providers { get; set; } = [];

	/// <summary>
	///     Enabled providers only when the global feature flag is enabled
	/// </summary>
	/// <returns>list of providers</returns>
	public List<ExternalAuthProviderSettings> GetEnabledProviders()
	{
		if ( !Enabled )
		{
			return [];
		}

		return Providers.Where(p => p.Enabled)
			.ToList();
	}

	/// <summary>
	///     Find a provider by id or provider type name (case-insensitive)
	/// </summary>
	/// <param name="provider">provider id or provider type</param>
	/// <returns>provider when found and enabled</returns>
	public ExternalAuthProviderSettings? GetEnabledProvider(string provider)
	{
		if ( string.IsNullOrWhiteSpace(provider) )
		{
			return null;
		}

		return GetEnabledProviders().FirstOrDefault(p =>
			string.Equals(p.Id, provider, StringComparison.OrdinalIgnoreCase) ||
			string.Equals(p.Provider, provider, StringComparison.OrdinalIgnoreCase));
	}
}

/// <summary>
///     Settings for one external OpenID Connect provider
/// </summary>
public class ExternalAuthProviderSettings
{
	/// <summary>
	///     Unique id (for example: "okta-main" or "azuread-main")
	/// </summary>
	public string Id { get; set; } = string.Empty;

	/// <summary>
	///     Enable or disable this provider
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	///     Provider type (for example: "Okta", "AzureAd", "Authentik")
	/// </summary>
	public string Provider { get; set; } = string.Empty;

	/// <summary>
	///     Optional display label for the login button
	/// </summary>
	public string DisplayName { get; set; } = string.Empty;

	/// <summary>
	///     OpenID Connect authority / discovery base
	/// </summary>
	public string Authority { get; set; } = string.Empty;

	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;

	/// <summary>
	///     Callback route path handled by Starsky
	/// </summary>
	public string CallbackPath { get; set; } = "/api/account/external-auth/callback";

	/// <summary>
	///     OIDC scopes
	/// </summary>
	public List<string> Scopes { get; set; } = [ "openid", "profile", "email" ];

	/// <summary>
	///     Multi-tenant configuration for this provider
	/// </summary>
	public List<ExternalAuthTenantSettings> Tenants { get; set; } = [];

	/// <summary>
	///     Optional claim name that contains role codes
	/// </summary>
	public string? RoleClaimType { get; set; }

	/// <summary>
	///     Optional claim name that contains groups
	/// </summary>
	public string? GroupClaimType { get; set; }
}

/// <summary>
///     Tenant specific settings for external providers
/// </summary>
public class ExternalAuthTenantSettings
{
	public string Id { get; set; } = string.Empty;
	public bool Enabled { get; set; } = true;
	public string Issuer { get; set; } = string.Empty;
	public string Audience { get; set; } = string.Empty;
}