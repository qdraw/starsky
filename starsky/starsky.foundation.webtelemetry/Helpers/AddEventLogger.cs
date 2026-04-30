using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Models;

namespace starsky.foundation.webtelemetry.Helpers;

internal class AddEventLogger
{
	private readonly Func<OSPlatform> _platformResolver = OperatingSystemHelper.GetPlatform;

	public AddEventLogger()
	{
	}

	internal AddEventLogger(Func<OSPlatform> platformResolver)
	{
		_platformResolver = platformResolver;
	}

	[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
	internal string AddEventLog(ILoggingBuilder logging,
		AppSettings.StarskyAppType type)
	{
		var sourceName = $"nl.qdraw.{type.ToString().ToLowerInvariant()}";
		if ( _platformResolver() == OSPlatform.Windows )
		{
			logging.AddEventLog(options => { options.SourceName = sourceName; });
		}

		return sourceName;
	}
}
