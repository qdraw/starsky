namespace starsky.foundation.webtelemetry.Helpers;

/// <summary>
///     Values of the type field of a breadcrumb. The Sentry sdk no longer ships these
///     constants, see https://develop.sentry.dev/sdk/event-payloads/breadcrumbs/
/// </summary>
internal static class BreadcrumbTypes
{
	internal const string Default = "default";
	internal const string Http = "http";
	internal const string Query = "query";
	internal const string Info = "info";
	internal const string Warning = "warning";
	internal const string Error = "error";
}
