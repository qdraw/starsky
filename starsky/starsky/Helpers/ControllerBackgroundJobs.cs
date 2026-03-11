using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.feature.geolookup.Interfaces;
using starsky.feature.realtime.Interface;
using starsky.foundation.database.Models;
using starsky.foundation.injection;
using starsky.foundation.metaupdate.Interfaces;
using starsky.foundation.metaupdate.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Interfaces;

namespace starsky.Helpers;

public static class ControllerBackgroundJobTypes
{
	public const string MetaTimeCorrect = "Controller.MetaTimeCorrect.v1";
}




public sealed class MetaTimeCorrectBackgroundPayload
{
	public List<ExifTimezoneCorrectionResult> ValidateResults { get; set; } = [];
	public string RequestType { get; set; } = string.Empty;
	public string RequestJson { get; set; } = string.Empty;
	public string CorrectionType { get; set; } = string.Empty;
}




