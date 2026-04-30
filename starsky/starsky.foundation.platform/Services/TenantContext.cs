using Microsoft.AspNetCore.Http;
using starsky.foundation.injection;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.platform.Services;

/// <summary>
///     Scoped implementation of <see cref="ITenantContext" />.
///
///     For HTTP requests the values are derived lazily from the current user's claims
///     (populated by TenantSessionAuthenticationMiddleware).  An explicit setter override
///     takes precedence — background-job code sets the properties directly after creating
///     a new DI scope, so the whole pipeline inside that scope sees the correct tenant.
/// </summary>
[Service(typeof(ITenantContext), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantContext : ITenantContext
{
	private readonly IHttpContextAccessor? _httpContextAccessor;
	private int? _tenantIdOverride;
	private bool _tenantIdOverrideSet;
	private string? _tenantSlugOverride;
	private bool _tenantSlugOverrideSet;

	public TenantContext(IHttpContextAccessor? httpContextAccessor = null)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	/// <inheritdoc />
	public int? TenantId
	{
		get
		{
			if ( _tenantIdOverrideSet )
			{
				return _tenantIdOverride;
			}

			var claim = _httpContextAccessor?.HttpContext?.User
				?.FindFirst(TenantConstants.TenantIdClaimType);
			return int.TryParse(claim?.Value, out var id) ? id : null;
		}
		set
		{
			_tenantIdOverride = value;
			_tenantIdOverrideSet = true;
		}
	}

	/// <inheritdoc />
	public string? TenantSlug
	{
		get
		{
			if ( _tenantSlugOverrideSet )
			{
				return _tenantSlugOverride;
			}

			// Prefer claim (set after auth middleware), fall back to HttpContext.Items (set by
			// TenantPathPrefixMiddleware which runs before auth).
			var claimSlug = _httpContextAccessor?.HttpContext?.User
				?.FindFirst(TenantConstants.TenantSlugClaimType)?.Value;
			if ( !string.IsNullOrEmpty(claimSlug) )
			{
				return claimSlug;
			}

			return _httpContextAccessor?.HttpContext?.Items
				.TryGetValue(TenantConstants.TenantSlugItemKey, out var item) == true
				? item as string
				: null;
		}
		set
		{
			_tenantSlugOverride = value;
			_tenantSlugOverrideSet = true;
		}
	}
}

