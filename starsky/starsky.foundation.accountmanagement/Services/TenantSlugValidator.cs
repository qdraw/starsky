using System;
using System.Text.RegularExpressions;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.injection;

namespace starsky.foundation.accountmanagement.Services;

[Service(typeof(ITenantSlugValidator), InjectionLifetime = InjectionLifetime.Scoped)]
public sealed class TenantSlugValidator : ITenantSlugValidator
{
	private static readonly Regex SlugRegex = new(
		"^[a-z0-9](?:[a-z0-9-]{1,48}[a-z0-9])?$",
		RegexOptions.Compiled,
		TimeSpan.FromMilliseconds(100));

	public bool IsValid(string? slug)
	{
		return !string.IsNullOrWhiteSpace(slug) && SlugRegex.IsMatch(slug);
	}
}
